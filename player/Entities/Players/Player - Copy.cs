﻿using System;
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

        public string GetMyPlayerName()
        {
            return $"player {m_team.m_teamName} {m_number}";
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

            var turnAngle = (angleToTarget - Convert.ToDouble(myAbsAngle));

            //Console.WriteLine($"turnAngle: {turnAngle}");

            var fixedAngle = NormalizeTo180(turnAngle);
            //Console.WriteLine($"fixedAngle: {fixedAngle}");

            return fixedAngle;
        }

        public double GetAngleTo0()
        {
            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var myAbsAngle = myPosByCoach.BodyAngle;
            myAbsAngle = -1 * myAbsAngle;
            if (m_side == 'l')
            {
                return NormalizeTo180(Convert.ToDouble(myAbsAngle));
            }
            else
            {
                return NormalizeTo180(Convert.ToDouble(myAbsAngle) + 180);
            }
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

        public double GetAngleToOpponentGoalUp()
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
            opponentGoalPos.Y -= 5;
            return GetAngleToPoint(opponentGoalPos);
        }

        public double GetAngleToOpponentGoalLow()
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
            opponentGoalPos.Y += 5;
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
                Console.WriteLine("Exception in wait step: " + e.Message);
            }
        }

        //--------------------------Higher Level API-------------------------
        public bool SpinAroundBall()
        {
            if (GetDistanceToBall() > DistFromBallToKick)
            {
                return true;
            }

                if((m_side == 'l' && GetMyPlayerDetailsByCoach().Pos.Value.X < GetBallDetailsByCoach().Pos.Value.X) ||
                   (m_side == 'r' && GetMyPlayerDetailsByCoach().Pos.Value.X > GetBallDetailsByCoach().Pos.Value.X))
                {
                    return true;
                }
                var ball = m_memory.GetSeenObject("ball");
            if (ball != null && Math.Abs(ball.Direction.Value) < 10)
            {
                if (m_side == 'l' && GetMyPlayerDetailsByCoach().Pos.Value.Y < GetBallDetailsByCoach().Pos.Value.Y)
                {
                    m_robot.Turn(NormalizeTo180(ball.Direction.Value + 90));
                    WaitSimulatorStep();
                    m_robot.Dash(15);
                    WaitSimulatorStep();
                    m_robot.Turn(-90);
                    return false;
                }
                else
                {
                    m_robot.Turn(NormalizeTo180(ball.Direction.Value - 90));
                    WaitSimulatorStep();
                    m_robot.Dash(15);
                    WaitSimulatorStep();
                    m_robot.Turn(90);
                    return false;
                }

            }
            else
            {
                m_robot.Dash(15);
                WaitSimulatorStep();
            }
            return true;

        }

        public bool PassToPossition(PointF targetPoint)
        {
            if (GetDistanceToBall() > DistFromBallToKick)
            {
                return true;
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
            WaitSimulatorStep();
            return true;
        }

        public void TurnToAngle0()
        {
            m_robot.Turn(GetAngleTo0());
            WaitSimulatorStep();
        }

        public void TurnToOpponentGoal()
        {
            m_robot.Turn(GetAngleToOpponentGoal());
            WaitSimulatorStep();
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
                    WaitSimulatorStep();
                    return false;
                }
                else
                {
                    return DashToPoint(point, trashHold);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in go to coordinate: " + e.Message);
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
                WaitSimulatorStep();
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
        public bool goToBallCoordinates(double trashHold, float beforeBallDistance = 0)
        {
            try
            {
                var ballPosByCoach = GetBallDetailsByCoach().Pos.Value;
                var myPosByCoach = GetMyPlayerDetailsByCoach().Pos.Value;
                if (m_side == 'l'){
                    if (ballPosByCoach.X < myPosByCoach.X)
                    {
                        ballPosByCoach.X -= beforeBallDistance * m_sideFactor;
                    }
                }
                else
                {
                    if (ballPosByCoach.X > myPosByCoach.X)
                    {
                        ballPosByCoach.X += beforeBallDistance;
                    }
                }

                var ballPosBySensors = m_memory.GetSeenObject("ball");
                if (ballPosBySensors == null)//We couldn't see the ball, go according to Coach directions
                {
                    return goToCoordinate(ballPosByCoach, trashHold);
                }
                if (Math.Abs(ballPosBySensors.Direction.Value) > 10)
                {
                    m_robot.Turn(ballPosBySensors.Direction.Value);
                    WaitSimulatorStep();
                    return false;
                }
                return DashToPoint(ballPosByCoach, trashHold);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in go to coordinate: " + e.Message);
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
            if (dist > 2)
            {
                m_robot.Dash(100);
                WaitSimulatorStep();
                WaitSimulatorStep();
            }
            else
            {
                m_robot.Dash(Math.Min(100, 20 * dist));
                WaitSimulatorStep();
            }
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
        public double GetDistanceToOpponentGoal()
        {
            PointF OpponentGoal;
            if (m_side == 'l')
            {
                OpponentGoal = (PointF)FlagNameToPointF.Convert("goal r");
            }
            else
            {
                OpponentGoal = (PointF)FlagNameToPointF.Convert("goal l");
            }
            return GetDistanceToPoint(OpponentGoal);
        }


        public double GetDistanceToMyOrigin()
        {
            return GetDistanceToPoint(m_startPosition);
        }

        public PointF? GetMostForwardPlayerPossition()
        {
            PointF? mostForwardPos = null;
            var seenObjects = m_coach.GetSeenCoachObjects();
            foreach (var seenObject in seenObjects)
            {
                if (seenObject.Key.StartsWith($"player {m_team.m_teamName}"))
                {
                    if (seenObject.Key.StartsWith($"player {m_team.m_teamName} 1"))
                    {
                        //It's Goalie, not relevant.
                        continue;
                    }
                    if (mostForwardPos == null)
                    {
                        mostForwardPos = seenObject.Value.Pos.Value;
                    }
                    else
                    {
                        if (m_side == 'l')
                        {
                            if (seenObject.Value.Pos.Value.X > mostForwardPos.Value.X)
                            {
                                mostForwardPos = seenObject.Value.Pos.Value;
                            }
                        }
                        else
                        {
                            if (seenObject.Value.Pos.Value.X < mostForwardPos.Value.X)
                            {
                                mostForwardPos = seenObject.Value.Pos.Value;
                            }
                        }

                    }
                }
            }
            return mostForwardPos;
        }

        public string GetMostForwardPlayerName()
        {
            PointF? mostForwardPos = null;
            string mostForwardName = null ;
            var seenObjects = m_coach.GetSeenCoachObjects();
            foreach (var seenObject in seenObjects)
            {
                if (seenObject.Key.StartsWith($"player {m_team.m_teamName}"))
                {
                    if (seenObject.Key.StartsWith($"player {m_team.m_teamName} 1"))
                    {
                        //It's Goalie, not relevant.
                        continue;
                    }
                    if (mostForwardPos == null)
                    {
                        mostForwardName = seenObject.Value.Name;
                        mostForwardPos = seenObject.Value.Pos.Value;
                    }
                    else
                    {
                        if (m_side == 'l')
                        {
                            if (seenObject.Value.Pos.Value.X > mostForwardPos.Value.X)
                            {
                                mostForwardName = seenObject.Value.Name;
                                mostForwardPos = seenObject.Value.Pos.Value;
                            }
                        }
                        else
                        {
                            if (seenObject.Value.Pos.Value.X < mostForwardPos.Value.X)
                            {
                                mostForwardName = seenObject.Value.Name;
                                mostForwardPos = seenObject.Value.Pos.Value;
                            }
                        }
                    }
                }
            }
            return mostForwardName;
        }

        public string GetMostBackwardPlayerName()
        {
            PointF? mostBackwardPos = null;
            string mostBackwardName = null;
            var seenObjects = m_coach.GetSeenCoachObjects();
            foreach (var seenObject in seenObjects)
            {
                if (seenObject.Key.StartsWith($"player {m_team.m_teamName}"))
                {
                    if (seenObject.Key.StartsWith($"player {m_team.m_teamName} 1"))
                    {
                        //It's Goalie, not relevant.
                        continue;
                    }
                    if (mostBackwardPos == null)
                    {
                        mostBackwardName = seenObject.Value.Name;
                        mostBackwardPos = seenObject.Value.Pos.Value;
                    }
                    else
                    {
                        if (m_side == 'l')
                        {
                            if (seenObject.Value.Pos.Value.X < mostBackwardPos.Value.X)
                            {
                                mostBackwardName = seenObject.Value.Name;
                                mostBackwardPos = seenObject.Value.Pos.Value;
                            }
                        }
                        else
                        {
                            if (seenObject.Value.Pos.Value.X > mostBackwardPos.Value.X)
                            {
                                mostBackwardName = seenObject.Value.Name;
                                mostBackwardPos = seenObject.Value.Pos.Value;
                            }
                        }
                    }
                }
            }
            return mostBackwardName;
        }

        public string GetClosestPlayerName()
        {
            double maxDist = 200;
            PointF? mostClosePlayerPoint = null;
            string mostClosePlayerName = null;
            var seenObjects = m_coach.GetSeenCoachObjects();
            foreach (var seenObject in seenObjects)
            {
                if (seenObject.Key.StartsWith($"player {m_team.m_teamName}"))
                {
                    if (seenObject.Key.StartsWith($"player {m_team.m_teamName} 1"))
                    {
                        continue;
                    }
                    if (Math.Abs(maxDist - 200) < 1 )
                    {
                        mostClosePlayerName = seenObject.Value.Name;
                        mostClosePlayerPoint = seenObject.Value.Pos.Value;
                        maxDist = GetDistanceBetween2Points(seenObject.Value.Pos.Value, GetBallDetailsByCoach().Pos.Value);
                    }
                    else
                    {
                        var curDist = GetDistanceBetween2Points(seenObject.Value.Pos.Value, GetBallDetailsByCoach().Pos.Value);
                        if (curDist < maxDist)
                        {
                            maxDist = curDist;
                            mostClosePlayerName = seenObject.Value.Name;
                            mostClosePlayerPoint = seenObject.Value.Pos.Value;
                        }
                    }
                }
            }
            return mostClosePlayerName;
        }


        public bool AmIMostForwarded()
        {
            return GetMyPlayerName() == GetMostForwardPlayerName();
        }

        public bool AmIMostBackward()
        {
            return GetMyPlayerName() == GetMostBackwardPlayerName();
        }


        public bool AmIClosest()
        {
            return GetMyPlayerName() == GetClosestPlayerName();
        }


        public void GoToOriginSynced()
        {
            while (!goToCoordinate(m_startPosition, 1)){}
        }

        public void RushBallSynced()
        {
            while (true)
            {
                if (goToBallCoordinates(1.5, m_sideFactor * 3)) break;
            }
        }

        //---------------------------Private Utils------------------------------
        private static double Calc2PointsAngleByXAxis(PointF start, PointF end)
        {
            return Math.Atan2(end.Y - start.Y, end.X - start.X) * Rad2Deg;
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
