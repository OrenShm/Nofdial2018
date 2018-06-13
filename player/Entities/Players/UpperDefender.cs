﻿using RoboCup.Entities;
using System;
using System.Drawing;
using System.Threading;

namespace RoboCup
{
    public class UpperDefender : Player
    {
        private const int WORKING_AREA = 30;
        private const int MOST_FORWARD_POSSITION = 5;
        private const int MOST_HEIGHT_DISTANCE = 15;


        private const int WAIT_FOR_MSG_TIME = 10;


        public UpperDefender(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 30, -20);
        }

        public override void play()
        {
            // first ,over to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);
            //Go to start possition in case the Move failed.
            GoToOriginSynced();
            while (!m_timeOver)
            {
                try
                {
                    if (GetDistanceToBall() > WORKING_AREA ||
                        GetMyPlayerDetailsByCoach().Pos.Value.Y > MOST_HEIGHT_DISTANCE ||
                        (GetMyPlayerDetailsByCoach().Pos.Value.X > MOST_FORWARD_POSSITION) && !AmIMostForwarded())
                    {
                        //GoToOriginSynced();
                        goToCoordinate(m_startPosition, 1);
                        WaitSimulatorStep();
                    }
                    else
                    {
                        //RushBallSynced();
                        if (goToBallCoordinates(1.5, m_sideFactor * 3) == false)
                        {
                            WaitSimulatorStep();
                            continue;
                        }
                        //WaitSimulatorStep();
                        if (GetMyPlayerDetailsByCoach().Pos.Value.X > 0)
                        {
                            if (AmIMostForwarded())
                            {
                                double angle = 0.0;
                                if (GetDistanceToOpponentGoal() < 10)
                                {
                                    angle = GetAngleToOpponentGoalUp();
                                }
                                else
                                {
                                    angle = GetAngleToOpponentGoal();
                                }
                                m_robot.Kick(100, angle);
                                WaitSimulatorStep();
                            }
                            else
                            {
                                PassToPossition((PointF)GetMostForwardPlayerPossition());
                                WaitSimulatorStep();
                            }
                        }
                        else
                        {
                            m_robot.Kick(30, 0);
                            WaitSimulatorStep();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in upper main loop: " + e.Message);
                }
            }




            // sleep one step to ensure that we will not send
            // two commands in one cycle.
            //WaitSimulatorStep();
        }

        private SenseBodyInfo GetBodyInfo()
        {
            m_robot.SenseBody();
            SenseBodyInfo bodyInfo = null;
            while (bodyInfo == null)
            {
                Thread.Sleep(WAIT_FOR_MSG_TIME);
                bodyInfo = m_memory.getBodyInfo();
            }

            return bodyInfo;
        }
    }
}
