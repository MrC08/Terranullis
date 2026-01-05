using System;
using System.Collections.Generic;
using Godot;

public class ChunkCompiler
{
	const int THREAD_COUNT = 32;

	GodotThread[] threads;

	public ChunkCompiler()
	{
		threads = new GodotThread[THREAD_COUNT];

		for (int i = 0; i < threads.Length; i++)
		{
			threads[i] = new GodotThread();
		}
	}

	public void run(List<ICompilable> chunksToGenerate, List<ICompilable> chunksToCompile)
	{
		int threadsRemaining = THREAD_COUNT;
		for (int i = 0; i < Math.Min(chunksToGenerate.Count, THREAD_COUNT); i++)
		{
			ICompilable chunk = chunksToGenerate[0];
			chunksToGenerate.RemoveAt(0);

			threads[i].Start(new Callable((Node3D) chunk, nameof(chunk.ThreadedGenerate)));
			threadsRemaining--;
		}	

		for (int i = THREAD_COUNT - threadsRemaining; i < Math.Min(chunksToCompile.Count, threadsRemaining); i++)
		{
			ICompilable chunk = chunksToCompile[0];
			chunksToCompile.RemoveAt(0);

			threads[i].Start(new Callable((Node3D) chunk, nameof(chunk.ThreadedCompile)));
		}
	}

	public void wait()
	{
		for (int i = 0; i < THREAD_COUNT; i++)
		{
			if (threads[i].IsAlive() || threads[i].IsStarted())
				threads[i].WaitToFinish();
		}
	}
}