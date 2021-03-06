﻿namespace Microshaoft
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public static class JObjectExtensionMethodsManager
    {
        private static void Travel
                                (
                                    JObject rootJObject
                                    , string ancestorPath
                                    , JObject currentJObject
                                    , Action<JObject, string, JObject, JProperty>
                                            onTraveledJValuePropertyProcessAction = null
                                    , Action<JObject, string, JObject>
                                            onTraveledJObjectPropertyProcessAction = null

                                )
        {
            onTraveledJObjectPropertyProcessAction?
                    .Invoke
                        (
                            rootJObject
                            , ancestorPath
                            , currentJObject
                        );
            var jProperties = currentJObject.Properties();
            foreach (var jProperty in jProperties)
            {
                var path = ancestorPath;
                if
                    (
                        !string.IsNullOrEmpty(path)
                        &&
                        !string.IsNullOrWhiteSpace(path)
                    )
                {
                    path += ".";
                }
                path += jProperty.Name;
                if (jProperty.HasValues)
                {
                    if
                        (
                            jProperty.Value is JValue
                        )
                    {

                        onTraveledJValuePropertyProcessAction?
                                .Invoke
                                    (
                                        rootJObject
                                        , ancestorPath
                                        , currentJObject
                                        , jProperty
                                    );
                    }
                    else if
                        (
                            jProperty.Value is JArray
                        )
                    {
                        var jArray = jProperty.Value as JArray;
                        var i = 0;
                        foreach (var jToken in jArray)
                        {
                            var jObject = jToken as JObject;
                            if (jObject != null)
                            {
                                Travel
                                    (
                                        rootJObject
                                        , string.Format("{0}[{1}]", path, i)
                                        , jObject
                                        , onTraveledJValuePropertyProcessAction
                                        , onTraveledJObjectPropertyProcessAction
                                    );
                            }
                            i++;
                        }
                    }
                    else
                    {
                        var jObject = jProperty.Value as JObject;
                        if (jObject != null)
                        {
                            Travel
                                (
                                    rootJObject
                                    , path
                                    , jObject
                                    , onTraveledJValuePropertyProcessAction
                                    , onTraveledJObjectPropertyProcessAction
                                );
                        }
                    }
                }
            }
        }


        public static void Travel
                                (
                                    this JObject target
                                    , Action<JObject, string, JObject, JProperty>
                                            onTraveledJValuePropertyProcessAction = null
                                    , Action<JObject, string, JObject>
                                            onTraveledJObjectPropertyProcessAction = null
                                )
        {
            Travel
                (
                    target
                    , string.Empty
                    , target
                    , onTraveledJValuePropertyProcessAction
                    , onTraveledJObjectPropertyProcessAction
                );
        }



        public static void Set
                                (
                                    this JObject target
                                    , string path
                                    , JValue value
                                    , Action
                                        <
                                            JObject
                                        >
                                            onPropertyChangedProcessAction
                                )
        {
            var arr = path.Split('.');
            JToken jToken = target[arr[0]];
            for (var i = 1; i < arr.Length - 1; i++)
            {
                jToken = jToken[arr[i]];
            }

            JObject j = jToken as JObject;
            j[arr[arr.Length - 1]] = value;

            if (onPropertyChangedProcessAction != null)
            {
                onPropertyChangedProcessAction(
                    target
                );
            }
        }
        public static void Observe
                                (
                                    this JObject target
                                    , Action
                                        <
                                            JObject
                                            , string
                                            , JObject
                                            , PropertyChangedEventArgs
                                            , JValue
                                            , JValue
                                        >
                                            onPropertyChangedProcessAction
                                )
        {

            //target.Annotation<Action<string>>().
            // HashSet --> Dictionary
            target.AddAnnotation(new Dictionary<string, JObject>());
            target.PropertyChanged += (
                (sender, args) =>
                {
                    if (!target.Annotation<Dictionary<string, JObject>>().ContainsKey(args.PropertyName))
                    {
                        JObject jDelegate = new JObject
                        (
                            new JProperty("methodName", onPropertyChangedProcessAction.Method.Name)
                            , new JProperty("isFlag", onPropertyChangedProcessAction == null ? true : false)
                        );
                        target.Annotation<Dictionary<string, JObject>>().Add(args.PropertyName, jDelegate);
                    }
                }

            );

            Travel
                (
                    target
                    , null
                    , (root, path, current) =>
                    {
                        JValue lastValue = null;
                        current
                            .PropertyChanging +=
                                            (
                                                (sender, args) =>
                                                {
                                                    var jo = sender as JObject;
                                                    if (jo != null)
                                                    {
                                                        lastValue = jo[args.PropertyName] as JValue;
                                                    }
                                                }
                                            );
                        JValue newValue = null;
                        current
                            .PropertyChanged +=
                                            (
                                                (sender, args) =>
                                                {
                                                    var jo = sender as JObject;
                                                    if (jo != null)
                                                    {
                                                        newValue = jo[args.PropertyName] as JValue;
                                                    }
                                                    onPropertyChangedProcessAction
                                                            (
                                                                root
                                                                , path
                                                                , current
                                                                , args
                                                                , lastValue
                                                                , newValue
                                                            );
                                                }
                                            );
                    }
                );
        }
    }
}


namespace TestConsoleApp6
{
    using Microshaoft;
    using Newtonsoft.Json.Linq;
    using System;
    class Program
    {
        static void Main(string[] args1)
        {
            string json = @"{
  'Name': 'Bad Boys',
  'ReleaseDate': '1995-4-7T00:00:00',
  'Genres': [
    {F1:'Action'},
    'Comedy'
  ]
,F4:{F1:'aa',F2:'ddd'}
}";

            json = "{Name:'asdsadas',F2:[1,{F1:'00000'}],F3: null, F4:{F5:{F6:{F7:'000',F8:'1'},F8:null}}}";
            var jObject = JObject.Parse(json);

            //jObject.Observe
            //            (
            //                (root, ancestorPath, current, args, v1, v2) =>
            //                {
            //                    Console.WriteLine("===================");
            //                    Console.WriteLine
            //                                (
            //                                    "{0}.{1}: ({2}) => ({3})"
            //                                    , ancestorPath
            //                                    , args.PropertyName
            //                                    , v1
            //                                    , v2
            //                                );
            //                    Console.WriteLine("===================");
            //                }
            //            );



            //jObject["name"] = "zzzz";
            //jObject["name"] = "1111";

            //Console.WriteLine(jObject.ToString());

            //jObject["F4"]["F5"]["F6"] = "zzzzA";

            //jObject[@"F9"] = "asdsa";

            //jObject["F2"][1]["F1"] = "zzzzA0000";


            //jObject["F2"][1]["F90"] = JObject.Parse("{'a':1,'b':2}");
            //jObject["F3"] = "zzzz";
            
            jObject.Set("F4.F5.F6.F7", new JValue("666"), null);

            Console.WriteLine(
                    jObject.ToString()
            );


            Console.ReadLine();
        }
    }
}