using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperFunctions
{
    public static List<T> JoinLists<T>(List<T> list1, List<T> list2)
    {
        // Create a new list that combines both lists
        List<T> combinedList = new List<T>(list1);
        combinedList.AddRange(list2);
        return combinedList;
    }
}
