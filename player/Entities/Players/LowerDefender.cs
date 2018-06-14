using RoboCup.Entities;
using RoboCup.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoboCup
{
    public class LowerDefender : Player
    {
        private const int WORKING_AREA = 35;
        private const int MOST_FORWARD_POSSITION = 5;
        private const int MOST_HEIGHT_DISTANCE = -15;


        private const int WAIT_FOR_MSG_TIME = 10;


        public LowerDefender(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 30, 20);
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
                        GetMyPlayerDetailsByCoach().Pos.Value.Y < MOST_HEIGHT_DISTANCE ||
                        ( GetMyPlayerDetailsByCoach().Pos.Value.X > MOST_FORWARD_POSSITION ) && !AmIMostForwarded())

                    {
                        //GoToOriginSynced();
                        goToCoordinate(m_startPosition, 1);
                        WaitSimulatorStep();
                    }
                    else
                    {
                        //RushBallSynced();
                        var ballXVal = GetBallDetailsByCoach().Pos.Value.X;
                        var ballYVal = GetBallDetailsByCoach().Pos.Value.Y;

                        var myXVal = GetMyPlayerDetailsByCoach().Pos.Value.X;
                        var myYVal = GetMyPlayerDetailsByCoach().Pos.Value.Y;

                        bool farEnough = ballXVal > myXVal && (Math.Abs(Math.Abs(ballXVal) - Math.Abs(myXVal)) > 10);
                        if (farEnough && Math.Abs(ballYVal - myYVal) > 4)
                        {
                            if (goToCoordinate(new PointF(myXVal, ballYVal), m_sideFactor * 3) == false)
                            {
                                WaitSimulatorStep();
                                continue;
                            }
                        }
                        else
                        {
                            if (goToBallCoordinates(1.5, m_sideFactor * 3) == false)
                            {
                                WaitSimulatorStep();
                                continue;
                            }
                        }

                        //WaitSimulatorStep();
                        if (GetMyPlayerDetailsByCoach().Pos.Value.X > 0)
                        {
                            if (AmIMostForwarded())
                            {
                                double angle = 0.0;
                                if (GetDistanceToOpponentGoal() < 10)
                                {
                                    if (GetMyPlayerDetailsByCoach().Pos.Value.Y > 0)
                                    {
                                        angle = GetAngleToOpponentGoalLow();
                                    }
                                    else
                                    {
                                        angle = GetAngleToOpponentGoalUp();
                                    }
                                }
                                else
                                {
                                    angle = GetAngleToOpponentGoal();
                                }
                                m_robot.Kick(100, angle);
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
                    Console.WriteLine("Exception in lower main loop: " + e.Message);
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
