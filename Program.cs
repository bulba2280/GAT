using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine("1 - Создать GAT файл");
        Console.WriteLine("2 - Прочитать GAT файл");
        Console.Write("Выбери: ");
        
        string choice = Console.ReadLine();
        
        if (choice == "1")
            CreateGAT();
        else if (choice == "2")
            ReadGAT();
    }
    
    // создание файла .gat
    static void CreateGAT()
    {
        Console.Write("НАпишите текст: ");
        string text = Console.ReadLine();
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        
        Console.Write("Напишите путь к гифке (Enter если нет): ");
        string gifPath = Console.ReadLine();
        
        using (FileStream file = new FileStream("output.gat", FileMode.Create))
        {
            // GAT!
            file.WriteByte(0x47);
            file.WriteByte(0x41);
            file.WriteByte(0x54);
            file.WriteByte(0x21);
            
            // Версия 1
            file.WriteByte(0x00);
            file.WriteByte(0x01);
            
            // Сколько блоков
            int blockCount = string.IsNullOrEmpty(gifPath) ? 1 : 2;
            WriteInt(file, blockCount);
            
            // Блок 1: текст
            file.WriteByte(0x00);  // тип 0 = текст
            WriteInt(file, textBytes.Length);
            file.Write(textBytes, 0, textBytes.Length);
            
            // Блок 2: гифка (если есть)
            if (!string.IsNullOrEmpty(gifPath) && File.Exists(gifPath))
            {
                byte[] gifBytes = File.ReadAllBytes(gifPath);
                file.WriteByte(0x01);  // тип 1 = гифка
                WriteInt(file, gifBytes.Length);
                file.Write(gifBytes, 0, gifBytes.Length);
            }
        }
        
        Console.WriteLine("чекай ;3");
    }
    
    // чтение .gat
    static void ReadGAT()
    {
        Console.Write("Путь к файлу: ");
        string path = Console.ReadLine();
        
        if (!File.Exists(path))
        {
            Console.WriteLine("Файл не найден");
            return;
        }
        
        using (FileStream file = new FileStream(path, FileMode.Open))
        {
            // Проверяем GAT!
            byte[] sig = new byte[4];
            file.Read(sig, 0, 4);
            if (Encoding.ASCII.GetString(sig) != "GAT!")
            {
                Console.WriteLine("Это не GAT файл!");
                return;
            }
            
            // Версия
            file.ReadByte();
            file.ReadByte(); 
            
            // Количество блоков
            int blockCount = ReadInt(file);
            Console.WriteLine($"Блоков в файле: {blockCount}\n");
            
            for (int i = 0; i < blockCount; i++)
            {
                int type = file.ReadByte();
                int length = ReadInt(file);
                byte[] data = new byte[length];
                file.Read(data, 0, length);
                
                if (type == 0) // текст
                {
                    Console.WriteLine($"[Блок {i+1}] Текст:");
                    Console.WriteLine(Encoding.UTF8.GetString(data));
                }
                else if (type == 1) // гифка
                {
                    string savePath = $"gif_{i+1}.gif";
                    File.WriteAllBytes(savePath, data);
                    Console.WriteLine($"[Блок {i+1}] Гифка сохранена: {savePath} ({length} байт)");
                }
            }
        }
    }
    
    
    static void WriteInt(FileStream file, int value)
    {
        file.WriteByte((byte)(value >> 24));
        file.WriteByte((byte)(value >> 16));
        file.WriteByte((byte)(value >> 8));
        file.WriteByte((byte)(value));
    }
    
    static int ReadInt(FileStream file)
    {
        byte[] b = new byte[4];
        file.Read(b, 0, 4);
        return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3];
    }
}