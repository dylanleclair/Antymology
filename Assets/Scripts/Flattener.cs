using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Extends the Array with the Flatten method
/// Taken from:
/// https://gist.github.com/jonasraoni/5ac06c17d096e2ac4117ca03a2f9244c
/// </summary>
public static class Flattener
{
	/// <summary>
	/// Given a N-dimensional array, flattens it into a new one-dimensional array without modifying the elements' order
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the array</typeparam>
	/// <param name="data">The input array</param>
	/// <returns>A flattened array</returns>
	public static T[] Flatten<T>(this Array data)
	{
		var list = new List<T>();
		var stack = new Stack<IEnumerator>();
		stack.Push(data.GetEnumerator());
		do
		{
			for (var iterator = stack.Pop(); iterator.MoveNext();)
			{
				if (iterator.Current is Array)
				{
					stack.Push(iterator);
					iterator = (iterator.Current as IEnumerable).GetEnumerator();
				}
				else
					list.Add((T)iterator.Current);
			}
		}
		while (stack.Count > 0);
		return list.ToArray();
	}

	// https://www.techiedelight.com/concatenate-two-arrays-csharp/
	public static T[] Concatenate<T>(this T[] first, T[] second)
	{
		if (first == null)
		{
			return second;
		}
		if (second == null)
		{
			return first;
		}

		return first.Concat(second).ToArray();
	}

}