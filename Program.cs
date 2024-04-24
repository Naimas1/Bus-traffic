using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static int maxBusCapacity = 50; // Максимальная вместимость автобуса
    static int maxPassengerCount = 200; // Максимальное количество пассажиров на остановке
    static int passengerCount = 0; // Текущее количество пассажиров на остановке

    static object lockObj = new object(); // Объект блокировки для синхронизации доступа к общим данным
    static Dictionary<int, AutoResetEvent> busEvents = new Dictionary<int, AutoResetEvent>(); // Словарь с событиями для каждого автобуса

    static ManualResetEvent stopEvent = new ManualResetEvent(false); // Событие для остановки генерации пассажиров

    static void Main(string[] args)
    {
        // Создаем потоки для автобусов
        for (int i = 1; i <= 5; i++) // Создаем 5 автобусов
        {
            busEvents.Add(i, new AutoResetEvent(false)); // Добавляем событие для каждого автобуса
            Thread busThread = new Thread(BusThread);
            busThread.Start(i); // Передаем номер автобуса как параметр потоку
        }

        // Создаем поток для генерации пассажиров
        Thread passengerThread = new Thread(GeneratePassengers);
        passengerThread.Start();

        Console.WriteLine("Имитация работы автобусной остановки начата...");
        Console.WriteLine("Нажмите любую клавишу для завершения работы...");
        Console.ReadKey();

        // Устанавливаем событие для остановки генерации пассажиров
        stopEvent.Set();

        // Освобождаем события для всех автобусов и ждем их завершения
        foreach (var busEvent in busEvents.Values)
        {
            busEvent.Set();
        }

        // Ожидаем завершения всех потоков
        passengerThread.Join();
    }

    static void GeneratePassengers()
    {
        Random rand = new Random();
        while (!stopEvent.WaitOne(0)) // Пока событие остановки не установлено
        {
            lock (lockObj)
            {
                // Генерируем случайное количество пассажиров
                int newPassengers = rand.Next(1, maxBusCapacity);
                Console.WriteLine($"На остановке появилось {newPassengers} пассажиров.");
                passengerCount += newPassengers;
                if (passengerCount > maxPassengerCount)
                    passengerCount = maxPassengerCount;
            }

            // Ожидаем случайное время перед приходом следующих пассажиров
            Thread.Sleep(rand.Next(500, 2000));
        }
    }

    static void BusThread(object busNumber)
    {
        Random rand = new Random();
        int busId = (int)busNumber;
        while (!stopEvent.WaitOne(0)) // Пока событие остановки не установлено
        {
            busEvents[busId].WaitOne(); // Ожидаем прибытия автобуса

            lock (lockObj)
            {
                // Подсчет количества пассажиров, которые смогут сесть в автобус
                int passengersInBus = Math.Min(passengerCount, maxBusCapacity);
                passengerCount -= passengersInBus;
                Console.WriteLine($"Автобус {busId}: Прибыл на остановку. Вместимость: {maxBusCapacity}, пассажиров: {passengersInBus}, осталось на остановке: {passengerCount}");
            }

            // Имитируем время, которое автобус проводит на остановке
            Thread.Sleep(rand.Next(1000, 5000));
        }
    }
}
