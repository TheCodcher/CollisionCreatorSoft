using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Reflection;
using System.Windows.Forms;

namespace ControlEngine
{
    class ObjectResurce<T>
    {
        public readonly string FileName;
        public ObjectResurce(string Name)
        {
            FileName = Name;
        }
        public T GetLoadedObject()
        {
            string res;
            try
            {
                res = LoadResurce.JSONInitializer[FileName];
            }
            catch
            {
                throw new Exception("The resource was not initialized");
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(res);
            }
            catch
            {
                throw new Exception("The resource can not deserialize");
            }
        }
        public Image GetGraphicResurce()
        {
            try
            {
                return LoadResurce.ImageInitializer[FileName];
            }
            catch
            {
                throw new Exception("The resource was not initialized");
            }
        }
        public void SaveResurce(string Path, T resurce)
        {
            LoadResurce.SaveJSON(Path, new KeyValuePair<string, string>(FileName, JsonConvert.SerializeObject(resurce)));
        }
    }
    static class LoadResurce
    {
        static public readonly string AssemblyDirPath = Application.StartupPath.ToString();
        static internal readonly Dictionary<string, string> JSONInitializer = new Dictionary<string, string>();
        static internal readonly Dictionary<string, Image> ImageInitializer = new Dictionary<string, Image>();

        static public event Action<float> InitializeProgression;

        static private void InitializeResurce(string Path, string[] Extension, Action<string, FileStream> InitializeHandler, bool UseSubdirectories)
        {
            InitializeProgression?.Invoke(0);
            Queue<DirectoryInfo> dirs = new Queue<DirectoryInfo>();
            List<FileInfo> Allfils = new List<FileInfo>();
            var dir = new DirectoryInfo(AssemblyDirPath + Path);
            if (!dir.Exists) throw new Exception("Path no correct");
            dirs.Enqueue(dir);
            while (dirs.Count != 0)
            {
                var d = dirs.Dequeue();
                Allfils.AddRange(d.GetFiles().Where((f) => Extension.Contains(f.Extension)));
                if (UseSubdirectories)
                {
                    dirs.Concat(d.GetDirectories());
                }
            }
            int progression = 0;
            foreach(var file in Allfils)
            {
                progression++;
                using (var fs = file.OpenRead())
                {
                    InitializeHandler(file.Name.Substring(0, file.Name.LastIndexOf('.')), fs);
                }
                InitializeProgression?.Invoke((float)progression / Allfils.Count * 100);
            }
        }
        static public void InitializeJSONResurce(string Path, bool UseSubdirectories = false)
        {
            void JSONHandler(string Name, FileStream filestream)
            {
                JSONInitializer.Add(Name, new StreamReader(filestream).ReadToEnd());
            }
            InitializeResurce(Path, new[] { ".json" }, JSONHandler, UseSubdirectories);
        }
        static public void InitializeImageResurce(string Path, bool UseSubdirectories = false)
        {
            void ImageHandler(string Name, FileStream filestream)
            {
                ImageInitializer.Add(Name, Image.FromStream(filestream));
            }
            InitializeResurce(Path, new[] { ".jpg", ".svg", ".png", ".bmp" }, ImageHandler, UseSubdirectories);
        }
        static public void SaveJSON(string Path, params KeyValuePair<string, string>[] SaveObjs)
        {
            //хрен его знает как это работает, может там вообще папка не создастся
            Path = AssemblyDirPath + Path;
            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);
            foreach(var ob in SaveObjs)
            {
                using (var jsonFile = File.CreateText(Path + @"\" + ob.Key + ".json"))
                {
                    jsonFile.Write(ob.Value);
                }
            }
        }
    }
}
