﻿using Gibbed.Dunia2.BinaryObjectInfo;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FCBConverter
{
    public class DefinitionsLoader
    {
        static List<DefFile> Files = new();
        static List<DefObject> Objects = new();

        public static void Parse(string file)
        {
            XDocument xmlDoc = XDocument.Load("a.xml");
            XElement root = xmlDoc.Element("FCBConverter");

            IEnumerable<XElement> xFiles = root.Elements("File");
            foreach (XElement xFile in xFiles)
            {
                DefFile defFile = new DefFile();
                defFile.Name = xFile.Attribute("name").Value;
                defFile.Objects = new();

                IEnumerable<XElement> xObjects = xFile.Elements("object");
                foreach (XElement xObject in xObjects)
                {
                    DefObject defObject = new DefObject();
                    defObject.Name = xObject.Attribute("name").Value;
                    defObject.Hash = xObject.Attribute("hash").Value;
                    defObject.Action = xObject.Attribute("action")?.Value;
                    defObject.Lists = new();
                    defObject.Fields = new();

                    IEnumerable<XElement> xConditionLists = xObject.Elements("ConditionList");
                    foreach (XElement xConditionList in xConditionLists)
                    {
                        string t = xConditionList.Attribute("type").Value;
                        _ = Enum.TryParse(t, out FieldType fieldType);

                        DefList defList = new DefList();
                        defList.Type = fieldType;
                        defList.ForceType = xConditionList.Attribute("forceType").Value;
                        defList.Action = xConditionList.Attribute("action")?.Value;
                        defList.Array = ParseConditionArray(xConditionList.Element("ConditionArray"));
                        defObject.Lists.Add(defList);
                    }

                    IEnumerable<XElement> xFields = xObject.Elements("field");
                    foreach (XElement xField in xFields)
                    {
                        string t = xField.Attribute("type").Value;
                        _ = Enum.TryParse(t, out FieldType fieldType);

                        DefField defField = new DefField();
                        defField.Hash = xField.Attribute("hash").Value;
                        defField.Name = xField.Attribute("name").Value;
                        defField.Type = fieldType;
                        defField.ForceType = xField.Attribute("forceType").Value;
                        defField.Action = xField.Attribute("action").Value;
                        defField.ListFromParent = xField.Attribute("listFromParent")?.Value;
                        defField.Comment = xField.Attribute("comment")?.Value;
                        defObject.Fields.Add(defField);
                    }

                    defFile.Objects.Add(defObject);
                }

                Files.Add(defFile);
            }

            foreach (DefFile defFile1 in Files)
            {
                if (defFile1.Name == "" || file.Contains(defFile1.Name))
                {
                    Objects.AddRange(defFile1.Objects);
                }
            }
        }

        private static DefConditionArray ParseConditionArray(XElement parent)
        {
            DefConditionArray a = new();
            a.Type = parent.Attribute("type").Value;
            a.Conditions = new();
            a.Arrays = new();

            IEnumerable<XElement> xConditions = parent.Elements("Condition");
            foreach (XElement xCondition in xConditions)
            {
                DefCondition cond = new();
                cond.Action = xCondition.Attribute("action").Value;
                cond.Value = xCondition.Attribute("value").Value;
                cond.Type = xCondition.Attribute("type")?.Value ?? "";
                a.Conditions.Add(cond);
            }

            IEnumerable<XElement> xConditionArrays = parent.Elements("ConditionArray");
            foreach (XElement xConditionArray in xConditionArrays)
            {
                a.Arrays.Add(ParseConditionArray(xConditionArray));
            }

            return a;
        }

        public static DefReturnVal Process(string objectName, string fieldHash, string binaryHex, string fieldName, string objectHash, string str, bool useLists)
        {
            DefReturnVal defReturnVal = new DefReturnVal();
            defReturnVal.Type = FieldType.BinHex;

            foreach (DefObject defObject in Objects)
            {
                if (
                    (defObject.Name != "" && defObject.Name == objectName) ||
                    (defObject.Hash != "" && defObject.Hash == objectHash) ||
                    (defObject.Name == "" && defObject.Hash == "")
                    )
                {
                    foreach (DefField defField in defObject.Fields)
                    {
                        if ((fieldHash != "" && defField.Hash == fieldHash) || (fieldName != "" && defField.Name == fieldName))
                        {
                            defReturnVal.Type = defField.Type;
                            defReturnVal.Action = defField.Action;
                            return defReturnVal;
                        }
                    }

                    if (useLists)
                    {
                        foreach (DefList defList in defObject.Lists)
                        {
                            bool p = Process(defList.Array, binaryHex, fieldName, str);
                            if (p)
                            {
                                defReturnVal.Type = defList.Type;
                                defReturnVal.Action = defList.Action;
                                return defReturnVal;
                            }
                        }
                    }
                }
            }

            return defReturnVal;
        }

        public static DefReturnVal ProcessObject(string objectName, string objectHash)
        {
            DefReturnVal defReturnVal = new DefReturnVal();
            defReturnVal.Type = FieldType.BinHex;

            foreach (DefObject defObject in Objects)
            {
                if (
                    (defObject.Name != "" && defObject.Name == objectName) ||
                    (defObject.Hash != "" && defObject.Hash == objectHash) ||
                    (defObject.Name == "" && defObject.Hash == "")
                    )
                {
                    defReturnVal.Action = defObject.Action;
                    return defReturnVal;
                }
            }

            return defReturnVal;
        }

        private static bool Process(DefConditionArray arr, string binaryHex, string name, string str)
        {
            bool useAnd = arr.Type == "and";

            bool tempCond = useAnd;

            foreach (DefCondition cond in arr.Conditions)
            {
                bool t = useAnd;

                if (cond.Action == "ByteLen")
                {
                    if (cond.Value.StartsWith(">"))
                        t = binaryHex.Length > int.Parse(cond.Value[1..]);
                    else if (cond.Value.StartsWith("<"))
                        t = binaryHex.Length < int.Parse(cond.Value[1..]);
                    else
                        t = binaryHex.Length == int.Parse(cond.Value);
                }
                if (cond.Action == "ByteLast")
                {
                    t = binaryHex.GetLast(cond.Value.Length) == cond.Value;
                }
                if (cond.Action == "StartsWithType")
                {
                    t = name.StartsWithType(cond.Value);
                }
                if (cond.Action == "StartsWith")
                {
                    t = name.StartsWith(cond.Value);
                }
                if (cond.Action == "EndsWith")
                {
                    t = name.EndsWith(cond.Value);
                }
                if (cond.Action == "ExactName")
                {
                    t = name == cond.Value;
                }
                if (cond.Action == "ExactValue")
                {
                    t = binaryHex == cond.Value;
                }
                if (cond.Action == "ContainsCI")
                {
                    t = name.ContainsCI(cond.Value);
                }
                if (cond.Action == "Contains")
                {
                    t = name.Contains(cond.Value);
                }
                if (cond.Action == "Regex")
                {
                    t = Regex.IsMatch(str, cond.Value);
                }
                if (cond.Action == "RegexBinaryHex")
                {
                    t = Regex.IsMatch(binaryHex, cond.Value);
                }

                if (cond.Type == "not")
                    t = !t;

                if (useAnd)
                    tempCond = tempCond && t;
                else
                    tempCond = tempCond || t;

                if (useAnd && !tempCond)
                    return tempCond;
            }

            foreach (DefConditionArray subArr in arr.Arrays)
            {
                bool t = Process(subArr, binaryHex, name, str);

                if (useAnd)
                    tempCond = tempCond && t;
                else
                    tempCond = tempCond || t;

                if (useAnd && !tempCond)
                    return tempCond;
            }

            return tempCond;
        }

        public class DefReturnVal
        {
            public FieldType Type { get; set; }

            public string Action { get; set; }

            public string Comment { get; set; }
        }

        public class DefFile
        {
            public string Name { get; set; }

            public List<DefObject> Objects { get; set; }
        }

        public class DefObject
        {
            public string Hash { get; set; }

            public string Name { get; set; }

            public string Action { get; set; }

            public List<DefList> Lists { get; set; }

            public List<DefField> Fields { get; set; }
        }

        public class DefField
        {
            public FieldType Type { get; set; }

            public string Hash { get; set; }

            public string Name { get; set; }

            public string ForceType { get; set; }

            public string Action { get; set; }

            public string Comment { get; set; }

            public string ListFromParent { get; set; }
        }

        public class DefList
        {
            public FieldType Type { get; set; }

            public string ForceType { get; set; }

            public string Action { get; set; }

            public DefConditionArray Array { get; set; }
        }

        public class DefCondition
        {
            public string Action { get; set; }

            public string Value { get; set; }

            public string Type { get; set; }
        }

        public class DefConditionArray
        {
            public string Type { set; get; }

            public List<DefCondition> Conditions { get; set; }

            public List<DefConditionArray> Arrays { get; set; }
        }
    }
}