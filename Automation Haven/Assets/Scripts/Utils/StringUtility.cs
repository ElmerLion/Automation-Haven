using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public static class StringUtility {
    public static string ReplacePlaceholders(string template, object values) {
        Type type = values.GetType();
        foreach (FieldInfo field in type.GetFields()) {
            template = template.Replace($"{{{field.Name}}}", field.GetValue(values).ToString());
        }
        foreach (PropertyInfo property in type.GetProperties()) {
            template = template.Replace($"{{{property.Name}}}", property.GetValue(values, null).ToString());
        }
        return template;
    }
}
