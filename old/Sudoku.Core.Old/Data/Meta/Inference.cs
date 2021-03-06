﻿using System;

namespace Sudoku.Data.Meta
{
	public readonly struct Inference : IEquatable<Inference>
	{
		public Inference(Candidate start, Candidate end, InferenceType inferenceType)
		{
			(Start, End, InferenceType) = (start, end, inferenceType);
		}


		public Candidate Start { get; }

		public Candidate End { get; }

		public InferenceType InferenceType { get; }


		public override bool Equals(object? obj)
		{
			return obj is Inference comparer && Equals(comparer);
		}

		public bool Equals(Inference other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode() => Start.GetHashCode() * 81 + End.GetHashCode();

		public override string? ToString()
		{
			return $"{Start} {(InferenceType == InferenceType.Strong ? "=" : "-")} {End}";
		}


		public static bool operator ==(Inference left, Inference right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Inference left, Inference right)
		{
			return !left.Equals(right);
		}
	}
}
