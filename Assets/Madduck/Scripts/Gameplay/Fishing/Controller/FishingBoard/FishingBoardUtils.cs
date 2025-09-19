using Madduck.Fishing.Shared;
using UnityEngine;

namespace Madduck.Fishing.Controller
{
    public static class FishingBoardUtils
    {
        /// <summary>
        /// Get the unit circle position from the object position.
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="position"></param>
        /// <returns>Unit circle position.</returns>
        public static Vector2 GetUnitCircle(this FishingBoardVariables variables, Vector2 position)
        {
            return position / variables.RedBoard.Radius;
        }
        
          /// <summary>
        /// Get a random position within the unit circle.
        /// </summary>
        /// <returns>Random position within the unit circle.</returns>
        public static Vector2 GetRandomPosition()
        {
            var randomPosition = Random.insideUnitCircle.normalized;
            return randomPosition;
        }

        /// <summary>
        /// Get a random position within the specified fish zone.
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="fishZone">The fish zone to get the random position from.</param>
        /// <returns>Random position within the specified fish zone.</returns>
        public static Vector2 GetRandomPositionOnFishZone(this FishingBoardVariables variables, FishZone fishZone)
        {
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var currentBoard = variables.CircleBoardState[fishZone];
            var previousBoard = variables.CircleBoardState[(FishZone)previousIndex];
            var currentThreshold = currentBoard.Radius / variables.RedBoard.Radius;
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / variables.RedBoard.Radius;
            var threshold = Random.Range(previousThreshold, currentThreshold);
            var randomPosition = Random.insideUnitCircle.normalized * threshold;
            return randomPosition;
        }
        
        /// <summary>
        /// Get the unit circle position from a target angle in degrees.
        /// </summary>
        /// <param name="angle">Target angle in degrees.</param>
        /// <returns>Unit circle position.</returns>
        public static Vector2 GetUnitCircleFromTargetAngle(float angle)
        {
            var radian = angle * Mathf.Deg2Rad;
            var x = Mathf.Cos(radian);
            var y = Mathf.Sin(radian);
            return new Vector2(x, y).normalized;
        }
        
        /// <summary>
        /// Get a random position within the specified unit circle by scaling the unit circle position with a random multiplier between 0 and 1.
        /// </summary>
        /// <param name="unitCircle">The unit circle position to scale.</param>
        /// <returns>Random position within the specified unit circle.</returns>
        public static Vector2 GetRandomPositionFromUnitCircle(Vector2 unitCircle)
        {
            var multiplier = Random.Range(0f, 1f);
            var randomPosition = unitCircle * multiplier;
            return randomPosition;
        }

        /// <summary>
        /// Get the fish zone based on the magnitude of the unit circle position.
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="unitCircleMagnitude">Magnitude of the unit circle position (0 to 1).</param>
        /// <returns>Fish zone.</returns>
        public static FishZone GetFishZone(this FishingBoardVariables variables, float unitCircleMagnitude)
        {
            var greenThreshold = variables.GreenBoard.Radius / variables.RedBoard.Radius;
            var yellowThreshold = variables.YellowBoard.Radius / variables.RedBoard.Radius;
            var redThreshold = variables.RedBoard.Radius / variables.RedBoard.Radius;
            if (unitCircleMagnitude <= greenThreshold)
            {
                return FishZone.Green;
            }
            if (unitCircleMagnitude <= yellowThreshold)
            {
                return FishZone.Yellow;
            }
            if (unitCircleMagnitude <= redThreshold)
            {
                return FishZone.Red;
            }
            return FishZone.Green;
        }

        /// <summary>
        /// Get the power multiplier based on the unit circle position.
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="unitCircle">Unit circle position.</param>
        /// <returns>Power multiplier.</returns>
        public static float GetPowerMultiplier(this FishingBoardVariables variables, Vector2 unitCircle)
        {
            var fishZone = variables.GetFishZone(unitCircle.magnitude);
            var index = (int)fishZone;
            var previousIndex = Mathf.Max(0, index - 1);
            var previousBoard = variables.CircleBoardState[(FishZone)previousIndex];
            var previousThreshold = previousIndex == index ? 0 : previousBoard.Radius / variables.RedBoard.Radius;
            var currentBoard = variables.CircleBoardState[fishZone];
            var lowerBound = currentBoard.MultiplierRange.x;
            var upperBound = currentBoard.MultiplierRange.y;
            var currentThreshold = currentBoard.Radius / variables.RedBoard.Radius;
            var relativePercent = (unitCircle.magnitude - previousThreshold) / (currentThreshold - previousThreshold);
            var multiplier = Mathf.Lerp(lowerBound, upperBound, relativePercent);
            return multiplier;
        }
    }
}