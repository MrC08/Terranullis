using System;
using Godot;

public class MeshSpan<T>
{
	private const int LENGTH_INC = 16384;

	private T[] arr;
	private int index;
	private int length;

	public MeshSpan()
	{
		index = 0;
		length = LENGTH_INC;
		arr = new T[length];
	}

	public void Add(T element)
	{
		if (index >= length)
		{
			length += LENGTH_INC;
			T[] newArr = new T[length];

			new Span<T>(arr).CopyTo(new Span<T>(newArr));

			arr = newArr;
		}

		arr[index] = element;
		index++;
	}

	public void AddQuadruplet(T element1, T element2, T element3, T element4)
	{
		if (index + 4 >= length)
		{
			length += LENGTH_INC;
			T[] newArr = new T[length];

			new Span<T>(arr).CopyTo(new Span<T>(newArr));

			arr = newArr;
		}

		arr[index] = element1;
		arr[index + 1] = element2;
		arr[index + 2] = element3;
		arr[index + 3] = element4;
		index += 4;
	}

	public void AddQuadruplet(T element)
	{
		if (index + 4 >= length)
		{
			length += LENGTH_INC;
			T[] newArr = new T[length];

			new Span<T>(arr).CopyTo(new Span<T>(newArr));

			arr = newArr;
		}

		arr[index] = element;
		arr[index + 1] = element;
		arr[index + 2] = element;
		arr[index + 3] = element;
		index += 4;
	}

	public void AddHextuplet(T element1, T element2, T element3, T element4, T element5, T element6)
	{
		if (index + 6 >= length)
		{
			length += LENGTH_INC;
			T[] newArr = new T[length];

			new Span<T>(arr).CopyTo(new Span<T>(newArr));

			arr = newArr;
		}

		arr[index] = element1;
		arr[index + 1] = element2;
		arr[index + 2] = element3;
		arr[index + 3] = element4;
		arr[index + 4] = element5;
		arr[index + 5] = element6;
		index += 6;
	}

	public Span<T> GetSpan()
	{
		return new Span<T>(arr).Slice(0, index);
	}
}