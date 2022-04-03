namespace System.Collections.Generic;

public static class ListExtension
{
    /// <summary>
    /// Swap the entries of <para>indexA</para> and <para>indexB</para>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="indexA"></param>
    /// <param name="indexB"></param>
    /// <returns></returns>
    public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
    {
        (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        return list;
    }
}