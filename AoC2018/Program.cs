using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AoC2018.Extensions;
using AoC2018.Helpers;
using TextCopy;

namespace AoC2018
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (Debugger.IsAttached == false)
            {
                ChristmasHelper.DrawWreathRibbonToConsole(0, 0, 6);

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Welcome, citizen!");
                Console.WriteLine();

                while (true)
                {
                    Console.WriteLine();
                    Console.WriteLine("Please enter 'exit', 'clear' or the # of the day you wish to see solved:");
                    var input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        Console.WriteLine("Your input is not valid, please try again!");
                        continue;
                    }

                    input = input.Trim().ToLower();

                    if (input == "exit")
                        return;

                    if (input == "clear")
                    {
                        Console.Clear();
                        continue;
                    }

                    if (int.TryParse(input, out var requestedDay) == false)
                    {
                        Console.WriteLine("Your requested day is not available, please try again!");
                        continue;
                    }

                    var type = Assembly.GetExecutingAssembly().GetType($"AoC2018.Day{requestedDay}");

                    if (!(Activator.CreateInstance(type) is IAoCDay day))
                        throw new Exception($"Type {type} is not a valid day");

                    Console.WriteLine();

                    Console.WriteLine($"Answers for day {requestedDay} - https://adventofcode.com/2018/day/{requestedDay}");
                    Console.WriteLine($"This day has been marked as: {day.Status}");

                    Console.WriteLine();

                    try
                    {
                        Console.WriteLine("Part 1:");
                        day.Part1();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }

                    try
                    {
                        Console.WriteLine("Part 2:");
                        day.Part2();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                    }
                }
            }

            new Day7().Part2();
            //new Day5().Part2();

            Console.ReadKey();
        }
    }

    public class Day7 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.WIP; }
        }

        public void Part1()
        {
            var input = InputHelper.GetInput(7);

            var steps = DiscoverSteps(input);
            var path = DiscoverPath(steps);

            Console.WriteLine(path);
        }


        public void Part2()
        {
            var input = InputHelper.GetInput(7);

            var steps = DiscoverSteps(input);
            var path = DiscoverPathForMultipleWorkers(steps, 5);

            Console.WriteLine(path);
        }

        private string DiscoverPath(IList<Step> steps)
        {
            return string.Join("", steps.Where(f => f.RequiredSteps.Count == 0).Select(ExecuteStep));
        }

        private string ExecuteStep(Step step)
        {
            step.HasBeenExecuted = true;

            var steps = string.Empty;

            while (step.RequiredForSteps.Any(f => f.CanBeExecuted && f.HasBeenExecuted == false)) steps += ExecuteStep(step.RequiredForSteps.First(f => f.CanBeExecuted && f.HasBeenExecuted == false));

            return step.Id + string.Join("", steps);
        }

        private string DiscoverPathForMultipleWorkers(IList<Step> steps, int workers)
        {
            foreach (var currentStep in steps) currentStep.TimeLeft = currentStep.TimeNeeded;

            var result = string.Empty;

            for (var i = 0; i < int.MaxValue; i++)
            {
                var stepsToExecuteNow = steps.SelectMany(DiscoverNextExecutableTimeBasedStep)
                    .ToHashSet()
                    .OrderBy(f => f.TimeLeft != f.TimeNeeded)
                    .Take(workers)
                    .ToList();

                if (stepsToExecuteNow.Count == 0)
                    break;

                var executedSteps = new List<Step>();
                foreach (var currentStepToExecute in stepsToExecuteNow)
                {
                    executedSteps.Add(currentStepToExecute);

                    currentStepToExecute.TimeLeft--;

                    if (currentStepToExecute.TimeLeft == 0)
                    {
                        currentStepToExecute.HasBeenExecuted = true;
                        result += currentStepToExecute.Id;

                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                }

                if (Debugger.IsAttached)
                    Console.WriteLine(i.FormatForDisplay() + "   "
                                                           + string.Join("   ", executedSteps.Select(f => $"{f.Id}({f.TimeLeft.FormatForDisplay()})"))
                                                           + AddWhiteSpaceForFormatting(workers - executedSteps.Count, "          ")
                                                           + "  " + result);

                Console.ResetColor();
            }

            return result;

            string AddWhiteSpaceForFormatting(int count, string @string)
            {
                var resultString = string.Empty;

                for (var i = 0; i < count; i++) resultString += @string;

                return resultString;
            }
        }

        private IEnumerable<Step> DiscoverNextExecutableTimeBasedStep(Step step)
        {
            var nextSteps = new List<Step>();

            if (step.CanBeExecuted && step.HasBeenExecuted == false)
                nextSteps.Add(step);

            var finishedSteps = step.RequiredForSteps.Where(f => f.HasBeenExecuted);

            foreach (var currentFinishedStep in finishedSteps) nextSteps.AddRange(DiscoverNextExecutableTimeBasedStep(currentFinishedStep));

            nextSteps.AddRange(step.RequiredForSteps.Where(f => f.HasBeenExecuted == false && f.CanBeExecuted));

            return nextSteps;
        }

        private IList<Step> DiscoverSteps(IList<string> input)
        {
            var parsedInput = input.Select(f => new
            {
                RequiredStepId = Regex.Match(f, "Step (?<Id>([A-Z]))").Groups["Id"].Value[0],
                StepId = Regex.Match(f, "before step (?<Id>([A-Z]))").Groups["Id"].Value[0]
            }).ToList();

            var steps = new List<Step>();

            foreach (var currentInput in parsedInput)
            {
                if (steps.Any(f => f.Id == currentInput.StepId) == false)
                    steps.Add(new Step
                    {
                        Id = currentInput.StepId,
                        RequiredSteps = new List<Step>(),
                        RequiredForSteps = new List<Step>()
                    });


                if (steps.Any(f => f.Id == currentInput.RequiredStepId) == false)
                    steps.Add(new Step
                    {
                        Id = currentInput.RequiredStepId,
                        RequiredSteps = new List<Step>(),
                        RequiredForSteps = new List<Step>()
                    });
            }

            steps = steps.OrderBy(f => f.Id).ToList();

            foreach (var currentStep in steps)
            foreach (var currentInput in parsedInput.Where(f => f.StepId == currentStep.Id).ToList())
            {
                var requiredStep = steps.First(f => f.Id == currentInput.RequiredStepId);

                currentStep.RequiredSteps.Add(requiredStep);
                requiredStep.RequiredForSteps.Add(currentStep);
            }

            return steps;
        }

        private class Step
        {
            public char Id { get; set; }

            public IList<Step> RequiredForSteps { get; set; }

            public IList<Step> RequiredSteps { get; set; }

            public bool HasBeenExecuted { get; set; }

            public bool CanBeExecuted
            {
                get { return RequiredSteps.All(f => f.HasBeenExecuted); }
            }

            public int TimeNeeded
            {
                get { return 61 + Id - 'A'; }
            }

            public int TimeLeft { get; set; }

            public override string ToString()
            {
                return $"{nameof(Id)} {Id}; " +
                       $" {nameof(CanBeExecuted)} {CanBeExecuted}; " +
                       $" {nameof(HasBeenExecuted)} {HasBeenExecuted}; ";
            }
        }
    }

    public class Day6 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.WIP; }
        }

        public void Part1()
        {
            var input = InputHelper.GetInput(6).Select(f =>
            {
                var result = Regex.Matches(f, "(\\d+)").Select(g => int.Parse(g.Value)).ToList();

                return new Point(result[0], result[1]);
            }).ToList();

            foreach (var currentPoint in input)
            {
            }
        }

        public void Part2()
        {
            throw new NotImplementedException();
        }
    }

    public class Day5 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.Completed; }
        }

        public void Part1()
        {
            Console.WriteLine("This might take a while...");

            var input = InputHelper.GetInput(5).First();
            var result = ReduceFully(input);

            Console.WriteLine($"Reduced input from {input.Length} to {result.Length}");
            Clipboard.SetText(input.Length.ToString());
        }

        public void Part2()
        {
            Console.WriteLine("This might take a while...");

            var input = InputHelper.GetInput(5).First();

            var results = new ConcurrentDictionary<char, string>();

            Parallel.ForEach(CharHelper.AllLowercaseLetters, f =>
            {
                var inputWithoutCurrentChar = input.Replace("" + f, "").Replace("" + char.ToUpper(f), "");
                var reduced = ReduceFully(inputWithoutCurrentChar);

                results.TryAdd(f, reduced);
            });

            foreach (var currentResult in results.OrderBy(f => f.Value.Length)) Console.WriteLine(currentResult.Key + " " + currentResult.Value.Length);

            Console.ReadKey();
        }

        private string ReduceFully(string input)
        {
            while (true)
            {
                var parts = new ConcurrentBag<string>();
                for (var i = 0; i < input.Length; i += 100) parts.Add(input.Substring(i, Math.Min(100, input.Length - i)));

                var anyWasReduced = false;
                var results = new ConcurrentBag<string>();
                Parallel.ForEach(parts, f =>
                {
                    var reduced = TryReduce(f, out var reducedResult);

                    results.Add(reducedResult);
                    anyWasReduced = reduced;
                });

                input = string.Join("", results);

                if (anyWasReduced == false)
                    break;
            }

            return input;
        }

        private bool TryReduce(string input, out string reducedInput)
        {
            var wasReduced = false;
            var i = 0;
            while (i < input.Length)
            {
                if (i + 1 == input.Length)
                    break;

                var value = input[i];
                var nextValue = input[i + 1];

                if (char.ToUpper(value) != char.ToUpper(nextValue) || value == nextValue)
                {
                    i++;
                    continue;
                }

                if (char.IsUpper(value) && char.IsLower(nextValue)
                    || char.IsLower(value) && char.IsUpper(nextValue))
                {
                    input = input.Remove(i, 2);
                    i = 0;
                    wasReduced = true;
                }
            }

            reducedInput = input;
            return wasReduced;
        }
    }

    public class Day4 : IAoCDay
    {
        public enum InputType
        {
            ShiftStart,
            FallingAsleep,
            WakingUp
        }

        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.Completed; }
        }

        public void Part1()
        {
            var input = ParseInput(InputHelper.GetInput(4)).OrderBy(f => f.Time).ToList();

            var guards = GetGuardSleepPatterns(input).OrderByDescending(f => f.TotalTimeAsleep).ToList();
            var laziestGuard = guards.First();

            Console.WriteLine(laziestGuard.GuardId * laziestGuard.MostAsleepMinute);
        }

        public void Part2()
        {
            var input = ParseInput(InputHelper.GetInput(4)).OrderBy(f => f.Time).ToList();

            var guards = GetGuardSleepPatterns(input)
                .OrderByDescending(f => f.MostAsleepMinuteSleepCount)
                .ToList();

            var laziestGuard = guards.First();

            Console.WriteLine(laziestGuard.GuardId * laziestGuard.MostAsleepMinute);
        }

        private IList<GuardSleepPattern> GetGuardSleepPatterns(IList<GuardInputValue> input)
        {
            var guards = new List<GuardSleepPattern>();

            foreach (var currentInput in input)
            {
                if (currentInput.Type != InputType.ShiftStart)
                    continue;

                var guardId = int.Parse(Regex.Match(currentInput.Value, "Guard #(?<id>(\\d+))").Groups["id"].Value);

                var existingSleepPattern = guards.FirstOrDefault(f => f.GuardId == guardId);

                if (existingSleepPattern == null)
                {
                    existingSleepPattern = new GuardSleepPattern
                    {
                        GuardId = guardId,
                        Patterns = new List<HardAtWorkTime>()
                    };
                    guards.Add(existingSleepPattern);
                }

                for (var i = input.IndexOf(currentInput) + 1; i < int.MaxValue; i += 2)
                {
                    if (i >= input.Count)
                        break;

                    var value1 = input[i];

                    if (value1.Type == InputType.ShiftStart)
                        break;

                    var value2 = input[i + 1];

                    existingSleepPattern.Patterns.Add(new HardAtWorkTime
                    {
                        StartDate = value1.Time,
                        EndDate = value2.Time
                    });
                }
            }

            return guards;
        }


        private IEnumerable<GuardInputValue> ParseInput(IList<string> inputs)
        {
            foreach (var currentInput in inputs)
            {
                var date = DateTime.Parse(currentInput.Substring(currentInput.IndexOf("[") + 1, currentInput.IndexOf("]") - currentInput.IndexOf("[") - 1));

                InputType? type = null;
                if (Regex.IsMatch(currentInput, "Guard #(\\d+)"))
                    type = InputType.ShiftStart;

                if (Regex.IsMatch(currentInput, "falls asleep"))
                    type = InputType.FallingAsleep;

                if (Regex.IsMatch(currentInput, "wakes up"))
                    type = InputType.WakingUp;

                if (type == null)
                    throw new ArgumentException("Input is invalid format");

                yield return new GuardInputValue
                {
                    Value = currentInput,
                    Time = date,
                    Type = type.Value
                };
            }
        }

        private class GuardInputValue
        {
            public DateTime Time { get; set; }

            public InputType Type { get; set; }

            public string Value { get; set; }
        }

        private class GuardSleepPattern
        {
            public int GuardId { get; set; }

            public IList<HardAtWorkTime> Patterns { get; set; }

            public int TotalTimeAsleep
            {
                get { return (int) Patterns.Sum(f => f.Duration.TotalMinutes); }
            }

            public int MostAsleepMinute
            {
                get { return GetTimesAsleepCount().OrderByDescending(f => f.Value).First().Key; }
            }

            public int MostAsleepMinuteSleepCount
            {
                get { return GetTimesAsleepCount().OrderByDescending(f => f.Value).First().Value; }
            }

            private Dictionary<int, int> GetTimesAsleepCount()
            {
                var sleepCounts = new Dictionary<int, int>();

                for (var i = 0; i < 60; i++)
                {
                    var sleepCount = 0;

                    foreach (var currentSleepTime in Patterns)
                        if (currentSleepTime.StartDate.Minute <= i && currentSleepTime.EndDate.Minute >= i)
                            sleepCount++;

                    sleepCounts.Add(i, sleepCount);
                }

                return sleepCounts;
            }

            public override string ToString()
            {
                return $"#{GuardId} {TotalTimeAsleep}";
            }
        }

        private class HardAtWorkTime
        {
            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }

            public TimeSpan Duration
            {
                get { return EndDate - StartDate; }
            }
        }
    }

    public class Day3 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.NO; }
        }

        public void Part1()
        {
            var input = ParseInput(InputHelper.GetInput(3)).ToList();

            var overlaps = new List<Overlap>();

            foreach (var currentRectangle in input)
            foreach (var currentRectangleToCompare in input.Where(f => ReferenceEquals(f, currentRectangle) == false
                                                                       && currentRectangle.Rectangle.IntersectsWith(f.Rectangle)))
            {
                var overlapsWith = overlaps.FirstOrDefault(f => f.IntersectsWith(currentRectangleToCompare.Rectangle));

                if (overlapsWith == null)
                {
                    overlaps.Add(new Overlap
                    {
                        InvolvedRectangles = new List<RectangleWithId>
                        {
                            currentRectangle,
                            currentRectangleToCompare
                        }
                    });
                }
                else
                {
                    if (overlapsWith.InvolvedRectangles.Contains(currentRectangle) == false)
                        overlapsWith.InvolvedRectangles.Add(currentRectangle);

                    if (overlapsWith.InvolvedRectangles.Contains(currentRectangleToCompare) == false)
                        overlapsWith.InvolvedRectangles.Add(currentRectangleToCompare);
                }
            }

            Console.WriteLine(overlaps.Sum(f => f.CalculateOverlap()));
            Console.ReadKey();
        }

        public void Part2()
        {
        }

        private IEnumerable<RectangleWithId> ParseInput(IList<string> input)
        {
            return input
                .Select(currentInput => Regex
                    .Matches(currentInput, "(\\d+)")
                    .Select(g => int.Parse(g.Value))
                    .ToList())
                .Select(numbers => new RectangleWithId
                {
                    Id = numbers[0],
                    Rectangle = new Rectangle(numbers[1], numbers[2], numbers[3], numbers[4])
                });
        }

        private class Overlap
        {
            public IList<RectangleWithId> InvolvedRectangles { get; set; }

            private Rectangle GetOverlapInternal()
            {
                Rectangle? overlapRectangle = null;

                for (var i = 0; i < InvolvedRectangles.Count - 1; i++) overlapRectangle = Rectangle.Intersect(InvolvedRectangles[i].Rectangle, overlapRectangle ?? InvolvedRectangles[i].Rectangle);

                if (overlapRectangle.HasValue == false)
                    throw new Exception("This group is bad");

                return overlapRectangle.Value;
            }

            public bool IntersectsWith(Rectangle rectangle)
            {
                return GetOverlapInternal().IntersectsWith(rectangle);
            }

            public int CalculateOverlap()
            {
                return GetOverlapInternal().Height * GetOverlapInternal().Width;
            }
        }

        private class RectangleWithId
        {
            public Rectangle Rectangle { get; set; }

            public int Id { get; set; }

            public override string ToString()
            {
                return "#" + Id + " " + Rectangle;
            }
        }
    }

    public class Day2 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.Completed; }
        }

        public void Part1()
        {
            var input = InputHelper.GetInput(2);

            var doubles = 0;
            var tripples = 0;

            foreach (var currentInput in input)
            {
                var isDouble = false;
                var isTripple = false;

                foreach (var currentLetter in CharHelper.AllLowercaseLetters)
                {
                    var count = currentInput.Count(f => f == currentLetter);

                    if (count == 2)
                        isDouble = true;

                    if (count == 3)
                        isTripple = true;
                }

                doubles += isDouble ? 1 : 0;
                tripples += isTripple ? 1 : 0;
            }

            Console.WriteLine(doubles * tripples);
        }

        public void Part2()
        {
            var input = InputHelper.GetInput(2);

            var matchingStrings = string.Empty;

            foreach (var currentInput in input)
            foreach (var currentComparedInput in input)
            {
                var differentLetters = currentInput.Length;
                var differentLetterPos = 0;

                for (var i = 0; i < currentInput.Length; i++)
                    if (currentInput[i] == currentComparedInput[i])
                        differentLetters -= 1;
                    else
                        differentLetterPos = i;

                if (differentLetters == 1) matchingStrings = currentInput.Remove(differentLetterPos, 1);
            }

            Console.WriteLine(matchingStrings);
        }
    }

    public class Day1 : IAoCDay
    {
        public AoCCompletionStatus Status
        {
            get { return AoCCompletionStatus.Completed; }
        }

        public void Part1()
        {
            var input = InputHelper.GetInput(1);

            var resultingFrequency = 0;

            foreach (var currentInput in input) resultingFrequency += int.Parse(currentInput);

            Console.WriteLine(resultingFrequency);
        }

        public void Part2()
        {
            var input = InputHelper.GetInput(1);
            var firstTwice = CalculateFirstFrequencyReacedTwice(input);

            Console.WriteLine(firstTwice);
        }

        private int CalculateFirstFrequencyReacedTwice(IList<string> input)
        {
            var frequencies = new List<int>();
            var resultingFrequency = 0;
            while (true)
                foreach (var currentInput in input)
                {
                    resultingFrequency += int.Parse(currentInput);

                    if (frequencies.Contains(resultingFrequency)) return resultingFrequency;

                    frequencies.Add(resultingFrequency);
                }
        }
    }

    public interface IAoCDay
    {
        AoCCompletionStatus Status { get; }

        void Part1();

        void Part2();
    }

    public enum AoCCompletionStatus
    {
        Completed,
        WIP,
        NotStarted,
        NO
    }
}