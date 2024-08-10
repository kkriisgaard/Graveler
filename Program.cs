using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Graveler
{
    internal class Program
    {
        // Number of sides of the die. Note that it will be substantially faster if it's a power of two.
        const uint NumSides = 4;
        const uint MaxRolls = 1_000_000_000;

        // You can probably use a bigger data type without and performance loss, and should in case of more turns than 255
        const byte RequiredProcs = 177;
        const byte NumTurns = 231;

        // Writing my own RNG, because System.Random() is kinda crap, both in terms of performance and randomness
        // The requirement for Xorshift is an initial state, which is nonzero. This upholds the requirement
        // This is probably stop working a very very long time in the futute, but should be doable for now.
        static ulong State = (ulong)DateTime.UtcNow.Ticks;

        static void Main(string[] args)
        {
            uint maxOnes = 0;
            uint bestAttempt = 0;

            Stopwatch sw = Stopwatch.StartNew();
            for (uint i = 0; i < MaxRolls; i++)
            {
                uint numOnes = 0;
                for (uint j = 0; j < NumTurns; j++)
                {
                    byte rollResult = GetRandomNumber();
                    if (rollResult == 0)
                        numOnes++;

                    if(numOnes == RequiredProcs)
                    {
                        ReportSuccess(i, j);
                        goto end; // Gotos may be considered harmful, but I can think of no better way to break nested loops.
                    }

                }

                if(numOnes > maxOnes)
                {
                    bestAttempt = i;
                    maxOnes = numOnes;
                }
            }

            sw.Stop();

        end:

            Console.WriteLine($"The simulation took {sw.Elapsed} for {MaxRolls} attemps");

            if (maxOnes < RequiredProcs)
                Console.WriteLine($"Graveler self-destructed. Best attempt was attempt number {bestAttempt}, where Graveler had a paralysis proc {maxOnes} times");
        }

        static void ReportSuccess(uint rollCount, uint turnCount)
        {
            Console.WriteLine($"Graveler did not self-destruct. Graveler had {RequiredProcs} paralysis procs after {turnCount} turns on attempt number {rollCount}");
        }

        // Beware: This works on a 64-bit number and returns an 8-bit number. This is cool because there are 4 sides. It wouldn't be if there were 1024
        // Ported from https://en.wikipedia.org/wiki/Xorshift
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte GetRandomNumber()
        {
            //ulong retVal = State;
            State ^= State << 13;
            State ^= State >> 7;
            State ^= State << 17;
            //State = retVal;
            return (byte)(State % NumSides);
        }
    }
}
