using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AutoServiceGame
{
    public class GameService : IDisposable
    {
        private readonly SqlConnection _conn;
        private readonly Random _rng = new Random();

        // Настраиваемые правила/коэффициенты
        private readonly decimal REFUSE_PENALTY_PERCENT = 0.5M;            // 50% от OfferedPrice при отказе
        private readonly decimal WRONG_REPLACEMENT_MULTIPLIER = 1.5M;     // 150% от OfferedPrice если заменили не ту деталь

        public GameService()
        {
            _conn = new SqlConnection("Data Source=bd-kip.fa.ru;Initial Catalog=Prokofiev_PR7;Persist Security Info=True;User ID=sa;Password=***********;Encrypt=False");
            _conn.Open();
        }

        public void Dispose()
        {
            _conn?.Dispose();
        }

        #region --- Вспомогательные DTO-методы ---

        public class PartInfo
        {
            public int PartId;
            public string Name;
            public decimal BuyPrice;
            public decimal WorkCost;
            public decimal RepairPrice;
        }

        public class InventoryItem
        {
            public int PartId;
            public int Quantity;
        }

        public class ClientJobInfo
        {
            public int JobId;
            public int ClientNumber;
            public int RequestedPartId;
            public decimal OfferedPrice;
        }

        #endregion

        #region --- Получения данных ---

        public decimal GetBalance()
        {
            using (var cmd = new SqlCommand("SELECT Balance FROM GameState WHERE Id = 1", _conn))
            {
                return (decimal)cmd.ExecuteScalar();
            }
        }

        public int GetNextClientNumber()
        {
            using (var cmd = new SqlCommand("SELECT NextClientNumber FROM GameState WHERE Id = 1", _conn))
            {
                return (int)cmd.ExecuteScalar();
            }
        }

        public List<PartInfo> GetAllParts()
        {
            var list = new List<PartInfo>();
            using (var cmd = new SqlCommand("SELECT PartId, Name, BuyPrice, WorkCost, RepairPrice FROM PartCatalog", _conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new PartInfo
                    {
                        PartId = (int)r["PartId"],
                        Name = (string)r["Name"],
                        BuyPrice = (decimal)r["BuyPrice"],
                        WorkCost = (decimal)r["WorkCost"],
                        RepairPrice = (decimal)r["RepairPrice"]
                    });
                }
            }
            return list;
        }

        public List<InventoryItem> GetInventory()
        {
            var list = new List<InventoryItem>();
            string sql = @"SELECT i.PartId, ISNULL(i.Quantity,0) AS Quantity
                           FROM PartCatalog p
                           LEFT JOIN Inventory i ON p.PartId = i.PartId";
            using (var cmd = new SqlCommand(sql, _conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new InventoryItem
                    {
                        PartId = (int)r["PartId"],
                        Quantity = (int)r["Quantity"]
                    });
                }
            }
            return list;
        }

        #endregion

        #region --- Клиент приходит: создание задания и применение пришедших заказов на склад ---

        /// <summary>
        /// Создаёт новый ClientJob (прибытие клиента) и возвращает DTO с информацией.
        /// Также перед созданием Job применяет все PurchaseOrders, у которых ArrivalClientNumber <= currentClientNumber.
        /// </summary>
        public ClientJobInfo CreateNextClientJob()
        {
            // Берём текущий nextClientNumber и список всех деталей (для случайного выбора поломки)
            int nextClientNumber = GetNextClientNumber();
            var parts = GetAllParts();
            if (parts.Count == 0) throw new InvalidOperationException("Нет деталей в каталоге.");

            // Случайно выбираем сломанную деталь
            var brokenPart = parts[_rng.Next(parts.Count)];

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    // 1) применяем поступившие purchase orders (ArrivalClientNumber <= nextClientNumber)
                    ApplyArrivedPurchaseOrdersInternal(nextClientNumber, tran);

                    // 2) создаём запись ClientJobs
                    string insertSql = @"INSERT INTO ClientJobs (ClientNumber, RequestedPartId, OfferedPrice, Accepted, Result)
                                         OUTPUT INSERTED.JobId
                                         VALUES (@ClientNumber, @RequestedPartId, @OfferedPrice, NULL, NULL)";
                    using (var cmd = new SqlCommand(insertSql, _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@ClientNumber", nextClientNumber);
                        cmd.Parameters.AddWithValue("@RequestedPartId", brokenPart.PartId);
                        cmd.Parameters.AddWithValue("@OfferedPrice", brokenPart.RepairPrice);
                        int newJobId = (int)cmd.ExecuteScalar();

                        // 3) инкрементируем GameState.NextClientNumber
                        using (var upd = new SqlCommand("UPDATE GameState SET NextClientNumber = NextClientNumber + 1 WHERE Id = 1", _conn, tran))
                        {
                            upd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return new ClientJobInfo
                        {
                            JobId = newJobId,
                            ClientNumber = nextClientNumber,
                            RequestedPartId = brokenPart.PartId,
                            OfferedPrice = brokenPart.RepairPrice
                        };
                    }
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Внутренняя функция: применяет все заказы, у которых ArrivalClientNumber <= currentClientNumber и которые ещё не Arrived.
        /// </summary>
        private void ApplyArrivedPurchaseOrdersInternal(int currentClientNumber, SqlTransaction tran)
        {
            // Берём все неприменённые заказы, готовые к приходу
            string selectSql = @"
                SELECT OrderId, PartId, Quantity
                FROM PurchaseOrders
                WHERE Arrived = 0 AND ArrivalClientNumber <= @CurrentClientNumber";
            var orders = new List<(int OrderId, int PartId, int Quantity)>();
            using (var cmd = new SqlCommand(selectSql, _conn, tran))
            {
                cmd.Parameters.AddWithValue("@CurrentClientNumber", currentClientNumber);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        orders.Add(((int)r["OrderId"], (int)r["PartId"], (int)r["Quantity"]));
                }
            }

            // Для каждого заказа — увеличить Inventory (INSERT или UPDATE) и пометить Arrived = 1
            foreach (var o in orders)
            {
                // обновляем Inventory
                string upsertSql = @"
                    IF EXISTS (SELECT 1 FROM Inventory WHERE PartId = @PartId)
                        UPDATE Inventory SET Quantity = Quantity + @Qty WHERE PartId = @PartId;
                    ELSE
                        INSERT INTO Inventory (PartId, Quantity) VALUES (@PartId, @Qty);";
                using (var cmd = new SqlCommand(upsertSql, _conn, tran))
                {
                    cmd.Parameters.AddWithValue("@PartId", o.PartId);
                    cmd.Parameters.AddWithValue("@Qty", o.Quantity);
                    cmd.ExecuteNonQuery();
                }

                // пометить Arrived = 1
                using (var cmd = new SqlCommand("UPDATE PurchaseOrders SET Arrived = 1 WHERE OrderId = @OrderId", _conn, tran))
                {
                    cmd.Parameters.AddWithValue("@OrderId", o.OrderId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region --- Действия с клиентом: принять / отказать ---

        /// <summary>
        /// Игрок принимает заказ. Возвращает строковое сообщение о результате.
        /// </summary>
        public string AcceptJob(int jobId)
        {
            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    // Получаем данные о задаче
                    string q = @"SELECT ClientNumber, RequestedPartId, OfferedPrice FROM ClientJobs WHERE JobId = @JobId";
                    int clientNumber;
                    int requestedPartId;
                    decimal offeredPrice;
                    using (var cmd = new SqlCommand(q, _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@JobId", jobId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) throw new InvalidOperationException("Job not found.");
                            clientNumber = (int)r["ClientNumber"];
                            requestedPartId = (int)r["RequestedPartId"];
                            offeredPrice = (decimal)r["OfferedPrice"];
                        }
                    }

                    // Применим поступившие заказы (на всякий случай) относительно clientNumber
                    ApplyArrivedPurchaseOrdersInternal(clientNumber, tran);

                    // Проверим, есть ли нужная деталь на складе
                    int qtyNeeded = 1;
                    int availableQty = 0;
                    using (var cmd = new SqlCommand("SELECT Quantity FROM Inventory WHERE PartId = @PartId", _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PartId", requestedPartId);
                        var res = cmd.ExecuteScalar();
                        if (res != null) availableQty = (int)res;
                    }

                    if (availableQty >= qtyNeeded)
                    {
                        // Успешный ремонт: снимаем деталь, добавляем деньги, помечаем Job
                        using (var cmd = new SqlCommand("UPDATE Inventory SET Quantity = Quantity - @Qty WHERE PartId = @PartId", _conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Qty", qtyNeeded);
                            cmd.Parameters.AddWithValue("@PartId", requestedPartId);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand("UPDATE GameState SET Balance = Balance + @Amount WHERE Id = 1", _conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Amount", offeredPrice);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(@"UPDATE ClientJobs 
                                                         SET Accepted = 1, Result = 'Repaired', UsedPartId = @UsedPartId, Penalty = 0
                                                         WHERE JobId = @JobId", _conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@UsedPartId", requestedPartId);
                            cmd.Parameters.AddWithValue("@JobId", jobId);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return $"Ремонт выполнен успешно. Баланс пополнен на {offeredPrice:C}.";
                    }
                    else
                    {
                        // Нужной детали нет. По правилам: если игрок принял, но нужной детали нет, ставится другая случайная деталь (если есть),
                        // и тогда игрок оплачивает компенсацию клиенту (штраф дороже чем при отказе).
                        // Ищем любую другую деталь на складе (qty > 0)
                        string findOtherSql = "SELECT TOP 1 PartId FROM Inventory WHERE Quantity > 0 AND PartId <> @Req ORDER BY NEWID()";
                        object otherPartObj;
                        using (var cmd = new SqlCommand(findOtherSql, _conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@Req", requestedPartId);
                            otherPartObj = cmd.ExecuteScalar();
                        }

                        if (otherPartObj == null)
                        {
                            // Нет ни одной детали — в таком случае принятый заказ не может быть выполнен: считаем как отказ
                            decimal penalty = Math.Round(offeredPrice * REFUSE_PENALTY_PERCENT, 2);
                            using (var cmd = new SqlCommand("UPDATE GameState SET Balance = Balance - @Penalty WHERE Id = 1", _conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Penalty", penalty);
                                cmd.ExecuteNonQuery();
                            }
                            using (var cmd = new SqlCommand(@"UPDATE ClientJobs 
                                                             SET Accepted = 1, Result = 'FailedNoParts', UsedPartId = NULL, Penalty = @Penalty
                                                             WHERE JobId = @JobId", _conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Penalty", penalty);
                                cmd.Parameters.AddWithValue("@JobId", jobId);
                                cmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                            return $"Вы приняли заказ, но в складе нет никаких деталей. Считался отказ — штраф {penalty:C}.";
                        }
                        else
                        {
                            int otherPartId = (int)otherPartObj;

                            // Снимаем одну случайную деталь (otherPartId)
                            using (var cmd = new SqlCommand("UPDATE Inventory SET Quantity = Quantity - 1 WHERE PartId = @PartId", _conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@PartId", otherPartId);
                                cmd.ExecuteNonQuery();
                            }

                            // Рассчитываем компенсацию (штраф)
                            decimal penalty = Math.Round(offeredPrice * WRONG_REPLACEMENT_MULTIPLIER, 2);

                            using (var cmd = new SqlCommand("UPDATE GameState SET Balance = Balance - @Penalty WHERE Id = 1", _conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@Penalty", penalty);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SqlCommand(@"UPDATE ClientJobs 
                                                             SET Accepted = 1, Result = 'WrongReplacement', UsedPartId = @UsedPartId, Penalty = @Penalty
                                                             WHERE JobId = @JobId", _conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@UsedPartId", otherPartId);
                                cmd.Parameters.AddWithValue("@Penalty", penalty);
                                cmd.Parameters.AddWithValue("@JobId", jobId);
                                cmd.ExecuteNonQuery();
                            }

                            tran.Commit();
                            return $"Нужной детали не было — поставлена случайная (PartId={otherPartId}). Клиент недоволен: выплочена компенсация {penalty:C}.";
                        }
                    }
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Игрок отказывается от заказа: снимаем штраф за отказ.
        /// </summary>
        public string RefuseJob(int jobId)
        {
            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    string q = "SELECT OfferedPrice FROM ClientJobs WHERE JobId = @JobId";
                    decimal offeredPrice;
                    using (var cmd = new SqlCommand(q, _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@JobId", jobId);
                        var obj = cmd.ExecuteScalar();
                        if (obj == null) throw new InvalidOperationException("Job not found.");
                        offeredPrice = (decimal)obj;
                    }

                    decimal penalty = Math.Round(offeredPrice * REFUSE_PENALTY_PERCENT, 2);

                    using (var cmd = new SqlCommand("UPDATE GameState SET Balance = Balance - @Penalty WHERE Id = 1", _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Penalty", penalty);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand(@"UPDATE ClientJobs 
                                                     SET Accepted = 0, Result = 'Refused', UsedPartId = NULL, Penalty = @Penalty
                                                     WHERE JobId = @JobId", _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Penalty", penalty);
                        cmd.Parameters.AddWithValue("@JobId", jobId);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    return $"Вы отказали клиенту. Штраф за отказ: {penalty:C}.";
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region --- Закупки (посредством PurchaseOrders) ---

        /// <summary>
        /// Сделать заказ на закупку: деньги списываются сразу, детали придут после 2 клиентов (ArrivalClientNumber = PlacedClientNumber + 2).
        /// </summary>
        public string PlacePurchaseOrder(int partId, int quantity)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive.");

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    // получаем цену покупки
                    decimal buyPrice;
                    using (var cmd = new SqlCommand("SELECT BuyPrice FROM PartCatalog WHERE PartId = @PartId", _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PartId", partId);
                        var obj = cmd.ExecuteScalar();
                        if (obj == null) throw new InvalidOperationException("Part not found.");
                        buyPrice = (decimal)obj;
                    }

                    decimal totalCost = Math.Round(buyPrice * quantity, 2);

                    // Узнаём текущ NextClientNumber (заметим: это номер следующего клиента, поэтому используем его как PlacedClientNumber)
                    int placedClientNumber;
                    using (var cmd = new SqlCommand("SELECT NextClientNumber FROM GameState WHERE Id = 1", _conn, tran))
                    {
                        placedClientNumber = (int)cmd.ExecuteScalar();
                    }

                    // Снимаем деньги сразу
                    using (var cmd = new SqlCommand("UPDATE GameState SET Balance = Balance - @Amount WHERE Id = 1", _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@Amount", totalCost);
                        cmd.ExecuteNonQuery();
                    }

                    // Добавляем запись в PurchaseOrders (ArrivalClientNumber вычисляется в БД как PlacedClientNumber + 2)
                    string insertSql = @"INSERT INTO PurchaseOrders (PartId, Quantity, PlacedClientNumber) 
                                         VALUES (@PartId, @Quantity, @PlacedClientNumber)";
                    using (var cmd = new SqlCommand(insertSql, _conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@PartId", partId);
                        cmd.Parameters.AddWithValue("@Quantity", quantity);
                        cmd.Parameters.AddWithValue("@PlacedClientNumber", placedClientNumber);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    return $"Заказ принят: PartId={partId}, Qty={quantity}. Списано {totalCost:C}. Детали прибудут после 2 клиентов (при ClientNumber >= {placedClientNumber + 2}).";
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region --- Утилиты для UI / отладки ---

        public ClientJobInfo GetJobById(int jobId)
        {
            string sql = "SELECT JobId, ClientNumber, RequestedPartId, OfferedPrice FROM ClientJobs WHERE JobId = @JobId";
            using (var cmd = new SqlCommand(sql, _conn))
            {
                cmd.Parameters.AddWithValue("@JobId", jobId);
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return null;
                    return new ClientJobInfo
                    {
                        JobId = (int)r["JobId"],
                        ClientNumber = (int)r["ClientNumber"],
                        RequestedPartId = (int)r["RequestedPartId"],
                        OfferedPrice = (decimal)r["OfferedPrice"]
                    };
                }
            }
        }

        public List<(int OrderId, int PartId, int Quantity, int PlacedClient, int ArrivalClient, bool Arrived)> GetPendingOrders()
        {
            var list = new List<(int, int, int, int, int, bool)>();
            string sql = "SELECT OrderId, PartId, Quantity, PlacedClientNumber, ArrivalClientNumber, Arrived FROM PurchaseOrders ORDER BY OrderId";
            using (var cmd = new SqlCommand(sql, _conn))
            using (var r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(((int)r["OrderId"], (int)r["PartId"], (int)r["Quantity"], (int)r["PlacedClientNumber"], (int)r["ArrivalClientNumber"], (bool)r["Arrived"]));
                }
            }
            return list;
        }

        #endregion
    }
}
