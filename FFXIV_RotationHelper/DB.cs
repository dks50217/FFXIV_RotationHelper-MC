﻿using CsvHelper;
using CsvHelper.Configuration;
using FFXIV_RotationHelper.StrongType;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FFXIV_RotationHelper
{
    using ActionTable = Dictionary<string, Dictionary<GameIdx, List<DBIdx>>>;

    public static class DB
    {
        private class ActionTableRow
        {
            public string ClassName { get; set; }
            public string ActionName { get; set; }
            public int GameIdx { get; set; }
            public int DBIdx { get; set; }
        };

        /// <summary>
        /// DB loaded from https://ffxivrotations.com/db.json
        /// </summary>
        private static readonly Dictionary<string, Dictionary<DBIdx, SkillData>> data = new Dictionary<string, Dictionary<DBIdx, SkillData>>();

        /// <summary>
        /// Is used to find DB using GameIdx
        /// </summary>
        private static readonly ActionTable actionTable = new ActionTable();

        /// <summary>
        /// Stores DBIdxes which is not supported (e.g. potions)
        /// </summary>
        private static readonly HashSet<DBIdx> ignoreSet = new HashSet<DBIdx>();

        public static bool IsLoaded { get; private set; }

        public static async Task LoadAsync()
        {
            await LoadAdjustTable();
            await LoadDB();

            IsLoaded = true;
        }

        private static async Task LoadAdjustTable()
        {

            string csvPath = "ActionTable.csv";
#if DEBUG
            csvPath = $@"{System.AppContext.BaseDirectory}\Data\ActionTable.csv";
#endif

            using (StreamReader streamReader = new StreamReader(File.OpenRead(@"C:\Users\User\Desktop\FFXIV_Rot\ActionTable.csv")))
            {
                string content = await streamReader.ReadToEndAsync();

                Task readTask = new Task(() => ReadCSV(content));
                readTask.Start();

                await readTask;
            }
        }

        private static void ReadCSV(string content)
        {
            CsvConfiguration configure = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                Quote = '\"',
            };

            using (StringReader reader = new StringReader(content))
            using (CsvReader csv = new CsvReader(reader, configure))
            {
                var recodes = csv.GetRecords<ActionTableRow>();
                foreach (ActionTableRow r in recodes)
                {
                    if (!actionTable.ContainsKey(r.ClassName))
                    {
                        actionTable.Add(r.ClassName, new Dictionary<GameIdx, List<DBIdx>>());
                    }

                    GameIdx gameIdx = (GameIdx)r.GameIdx;
                    if (!actionTable[r.ClassName].ContainsKey(gameIdx))
                    {
                        actionTable[r.ClassName].Add(gameIdx, new List<DBIdx>());
                    }

                    DBIdx dbIdx = (DBIdx)r.DBIdx;
                    actionTable[r.ClassName][gameIdx].Add(dbIdx);
                }
            }
        }

        private static async Task LoadDB()
        {
            string dbUrl = string.Empty;

            { // Get DB url
                HttpWebRequest request = WebRequest.Create("https://raw.githubusercontent.com/Elysia-ff/FFXIV_RotationHelper-resources/master/Output/dburl.txt") as HttpWebRequest;
                using (HttpWebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null) as HttpWebResponse)
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    dbUrl = await streamReader.ReadToEndAsync();
                }
            }

            { // Load DB 
                HttpWebRequest request = WebRequest.Create(dbUrl) as HttpWebRequest;
                using (HttpWebResponse response = await Task.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null) as HttpWebResponse)
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                {
                    string content = await streamReader.ReadToEndAsync();
                    JObject jObject = JObject.Parse(content);
                    JToken skills = jObject.GetValue("skills");
                    JToken classes = jObject.GetValue("classes");

                    foreach (JProperty classProperty in classes.Children<JProperty>())
                    {
                        string discipline = classProperty.Value.Value<string>("discipline");
                        if (discipline != "war" && discipline != "magic")
                        {
                            continue;
                        }

                        string className = classProperty.Name;
                        if (!data.ContainsKey(className))
                        {
                            data.Add(className, new Dictionary<DBIdx, SkillData>());
                        }

                        foreach (JProperty skillProperty in classProperty.Value.Children<JProperty>())
                        {
                            if (skillProperty.Value.Type != JTokenType.Array)
                            {
                                continue;
                            }

                            foreach (int idx in skillProperty.Value.Values<int>())
                            {
                                JObject skillObject = skills.Value<JObject>(idx.ToString());

                                string deprecatedField = skillObject.Value<string>("deprecated");
                                if (string.IsNullOrEmpty(deprecatedField) || deprecatedField == "0")
                                {
                                    DBIdx dbIdx = (DBIdx)idx;
                                    SkillData skillData = new SkillData(dbIdx, skillObject);
                                    data[className].Add(dbIdx, skillData);
                                }
                            }
                        }
                    }

                    JToken misc = jObject.GetValue("misc");
                    foreach (JValue jValue in misc.Children<JValue>())
                    {
                        int idx = jValue.Value<int>();
                        ignoreSet.Add((DBIdx)idx);
                    }
                }
            }

#if DEBUG
            Debug.WriteLine("Skill Count : " + data.Count);
#endif
        }

        public static List<SkillData> Get(RotationData rotationData)
        {
            if (rotationData.Class == null || !data.ContainsKey(rotationData.Class) || rotationData.Sequence == null || rotationData.Sequence.Count <= 0)
            {
                return new List<SkillData>();
            }

            List<SkillData> list = new List<SkillData>();
            for (int i = 0; i < rotationData.Sequence.Count; ++i)
            {
                if (data[rotationData.Class].TryGetValue(rotationData.Sequence[i], out SkillData skillData))
                {
                    list.Add(skillData);
                }
            }
            
            return list;
        }

        public static bool IsSameAction(string className, GameIdx gameIdx, DBIdx dBIdx)
        {
            if (actionTable.ContainsKey(className) && actionTable[className].ContainsKey(gameIdx))
            {
                List<DBIdx> idxes = actionTable[className][gameIdx];
                for (int i = 0; i < idxes.Count; i++)
                {
                    if (idxes[i] == dBIdx)
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        public static bool IsIgnoreSet(DBIdx dBIdx)
        {
            return ignoreSet.Contains(dBIdx);
        }

#if DEBUG
        public static List<SkillData> Find(string actionName)
        {
            try
            {
                actionName = actionName.Replace(" ", "").ToLower();

                List<SkillData> list = new List<SkillData>();
                foreach (var d in data)
                {
                    foreach (KeyValuePair<DBIdx, SkillData> kv in data[d.Key])
                    {
                        string str = kv.Value.Name.Replace(" ", "").ToLower();
                        if (str.Equals(actionName))
                        {
                            list.Add(kv.Value);
                        }
                    }
                }

                return list;
            }
            catch
            {
                return new List<SkillData>();
            }
        }
#endif
    }
}
