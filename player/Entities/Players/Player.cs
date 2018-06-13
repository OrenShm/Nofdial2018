using System;
using System.Drawing;
using System.Threading;
using RoboCup;
using RoboCup.Entities;
using RoboCup.Infrastructure;

namespace RoboCup
{

    public class Player : IPlayer
    {
        protected const double DistFromBallToKick = 1.7;

        protected const double Rad2Deg = 180.0 / Math.PI;
        protected const double Deg2Rad = Math.PI / 180.0;
        // Protected members
        protected Robot m_robot;			    // robot which is controled by this brain
        protected Memory m_memory;				// place where all information is stored
        protected PointF m_startPosition;
        volatile protected bool m_timeOver;
        protected Thread m_strategy;
        protected int m_sideFactor
        {
            get
            {
                return m_side == 'r' ? 1 : -1;
            }
        }

        // Public members
        public int m_number;
        public char m_side;
        public String m_playMode;
        public Team m_team;
        public ICoach m_coach;

        public Player(Team team, ICoach coach , bool IsGoalie = false)
        {
            m_coach = coach;
            m_memory = new Memory();
            m_team = team;
            m_robot = new Robot(m_memory);
            m_robot.Init(team.m_teamName, out m_side, out m_number, out m_playMode, IsGoalie);
            
            Console.WriteLine("New Player - Team: " + m_team.m_teamName + " Side:" + m_side +" Num:" + m_number);

            m_strategy = new Thread(new ThreadStart(play));
            m_strategy.Start();
        }

        public virtual  void play()
        {
 
        }

        //------------------------------Public util functions---------------------

        public SeenCoachObject GetMyPlayerDetailsByCoach()
        {
            var res =  m_coach.GetSeenCoachObject($"player {m_team.m_teamName} {m_number}");
            if (res == null)
            {
                throw new Exception("Couldn't find my player");
            }
            return res;
        }

        public SeenCoachObject GetBallDetailsByCoach()
        {
            var res = m_coach.GetSeenCoachObject($"ball");
            if (res == null)
            {
                throw new Exception("Couldn't find the ball");
            }
            return res;
        }


        public double GetAngleToPoint(PointF targetPoint)
        {
            //Console.WriteLine("----------------------------------------");

            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var angleToTarget = Calc2PointsAngleByXAxis(myPosByCoach.Pos.Value, targetPoint);
            var myAbsAngle = myPosByCoach.BodyAngle;

            //Console.WriteLine($"myAbsAngle: {myAbsAngle}");
            //Console.WriteLine($"angleToTarget: {angleToTarget}");

            var turnAngle = -1 * (Convert.ToDouble(myAbsAngle) + angleToTarget);
            //Console.WriteLine($"turnAngle: {turnAngle}");

            var fixedAngle = NormalizeTo180(turnAngle);
            //Console.WriteLine($"fixedAngle: {fixedAngle}");


            return turnAngle;
        }

        public double GetAngleTo0()
        {
            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var myAbsAngle = myPosByCoach.BodyAngle;
            myAbsAngle = -1 * myAbsAngle;
            return NormalizeTo180(Convert.ToDouble(myAbsAngle));
        }

        public double GetAngleToOpponentGoal()
        {
            PointF opponentGoalPos;
            if (m_side == 'l')
            {
                opponentGoalPos = (PointF)FlagNameToPointF.Convert("goal r");
            }
            else
            {
                opponentGoalPos = (PointF)FlagNameToPointF.Convert("goal l");
            }
            return GetAngleToPoint(opponentGoalPos);
        }

        public static void WaitSimulatorStep()
        {
            try
            {
                Thread.Sleep(SoccerParams.simulator_step);
            }
            catch (Exception e)
            {

            }
        }

        //--------------------------Higher Level API-------------------------
        public bool PassToPossition(PointF targetPoint)
        {
            if (GetDistanceToBall() > DistFromBallToKick)
            {
                throw new Exception("Too far from ball to shot");
            }
            var dist = GetDistanceToPoint(targetPoint);
           
            if (dist > 30) //Too far, shot at max power
            {
                m_robot.Kick(100, GetAngleToPoint(targetPoint));
            }
            else if (dist > 10)
            {
                m_robot.Kick(Math.Min(100, GetDistanceToPoint(targetPoint) * 7), GetAngleToPoint(targetPoint));
            }
            else //Very close
            {
                m_robot.Kick(Math.Min(100, GetDistanceToPoint(targetPoint) * 3), GetAngleToPoint(targetPoint));
            }
            return true;
        }

        public void TurnToAngle0()
        {
            m_robot.Turn(GetAngleTo0());
        }

        public void TurnToOpponentGoal()
        {
            m_robot.Turn(GetAngleToOpponentGoal());
        }

        /// <summary>
        /// Goes to requested coordinate
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns> true in case we reached the requested pos, false otherwise</returns>
        public bool goToCoordinate(PointF point, double trashHold)
        {
            try
            {
                var dist = GetDistanceToPoint(point);
                if (dist < trashHold)
                {
                    return true;
                }
                var turnAngle = GetAngleToPoint(point);
                if (Math.Abs(turnAngle) > 10)
                {
                    m_robot.Turn(turnAngle);
                    return false;
                }
                else
                {
                    return DashToPoint(point, trashHold);
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool goToCoordinate(PointF point, double trashHold, PointF topLimitPoint, PointF BottomLimitPoint)
        {
            PointF limitedPoint = point;
            if (m_side == 'l')
            {
                if (point.X > topLimitPoint.X)
                {
                    limitedPoint.X = topLimitPoint.X;
                }
            }
            else
            {
                if (point.X < topLimitPoint.X)
                {
                    limitedPoint.X = topLimitPoint.X;
                }
            }

            if (point.Y < topLimitPoint.Y)
            {
                limitedPoint.Y = topLimitPoint.Y;
            }
            else if (point.Y > BottomLimitPoint.Y)
            {
                limitedPoint.Y = BottomLimitPoint.Y;
            }
            

            var dist = GetDistanceToPoint(limitedPoint);
            if (dist < trashHold)
            {
                return true;
            }
            var turnAngle = GetAngleToPoint(limitedPoint);
            if (Math.Abs(turnAngle) > 10)
            {
                m_robot.Turn(turnAngle);
                return false;
            }
            else
            {
                return DashToPoint(limitedPoint, trashHold);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true in case we got to the ball</returns>
        public bool goToBallCoordinates(double trashHold)
        {
            try
            {
                var ballPosByCoach = GetBallDetailsByCoach().Pos.Value;
                var ballPosBySensors = m_memory.GetSeenObject("ball");
                if (ballPosBySensors == null)//We couldn't see the ball, go according to Coach directions
                {
                    return goToCoordinate(ballPosByCoach, trashHold);
                }
                if (Math.Abs(ballPosBySensors.Direction.Value) > 10)
                {
                    m_robot.Turn(ballPosBySensors.Direction.Value);
                    return false;
                }
                return DashToPoint(ballPosByCoach, trashHold);
            }
            catch (Exception e)
            {
                return false;
            }

        }

        /// <summary>
        /// Dashes to targetPoint.
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <returns>true when target is reached</returns>
        public bool DashToPoint(PointF targetPoint, double trashHold)
        {
            double dist = GetDistanceToPoint(targetPoint);
            if (dist < trashHold)
            {
                return true;
            }
            m_robot.Dash(Math.Min(100, 20 * dist));
            return false;
        }

        public double GetDistanceToPoint(PointF targetPoint)
        {
            var myPlayerDetails = GetMyPlayerDetailsByCoach();
            return GetDistanceBetween2Points(myPlayerDetails.Pos.Value, targetPoint);
        }

        public double GetDistanceBetween2Points(PointF sourcePoint, PointF targetPoint)
        {
            return Math.Sqrt(Math.Pow(sourcePoint.X - targetPoint.X, 2) + Math.Pow(sourcePoint.Y - targetPoint.Y, 2));
        }

        public double GetDistanceToBall()
        {
            return GetDistanceToPoint(GetBallDetailsByCoach().Pos.Value);
        }

        public double GetDistanceToMyOrigin()
        {
            return GetDistanceToPoint(m_startPosition);
        }

        //---------------------------Private Utils------------------------------
        private static double Calc2PointsAngleByXAxis(PointF start, PointF end)
        {
            return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg;
        }

        private static double NormalizeTo180(double angle)
        {
            while (Math.Abs(angle) > 180)
            {
                if (angle > 0)
                {
                    angle = angle - 360;
                }
                else
                {
                    angle = angle + 360;
                }
            }

            return angle;
        }

    }
}
