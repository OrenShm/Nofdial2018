﻿using RoboCup.Entities;
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
    public class Forward : Player
    {
        public const int MOST_BACKWARD_POSSITION = 10;
        public const int MOST_HEIGHT_DISTANCE = 5;
        public const int WAIT_FOR_MSG_TIME = 10;

        public virtual bool OverX
        {
            get
            {
                if (m_side == 'l')
                {
                    return GetBallDetailsByCoach().Pos.Value.X > MOST_BACKWARD_POSSITION;
                }
                return GetBallDetailsByCoach().Pos.Value.X < MOST_BACKWARD_POSSITION * -1;
            }
        }

        public virtual bool OverY
        {
            get
            {
                return false;
            }
        }


        public Forward(Team team, ICoach coach)
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 10, 0);
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
                    if (!AmIClosest() && (OverY || (OverX && !AmIMostBackward())))
                    {
                        //GoToOriginSynced();
                        //                        Console.WriteLine($"Player {m_number} going back (amIclosest = {AmIClosest()}) ");
                        //                      Console.WriteLine($"OrigPoint: {m_startPosition.X},{m_startPosition.X} ");

                        goToCoordinate(m_startPosition, 1);
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
                            if (goToCoordinate(new PointF(myXVal, ballYVal), m_sideFactor * 0) == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (goToBallCoordinates(1.5, m_sideFactor * 0) == false)
                            {
                                continue;
                            }
                        }

                        //WaitSimulatorStep();

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
                            if (!SpinAroundBall()) continue;
                            m_robot.Kick(100, angle);
                            WaitSimulatorStep();

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
    }
}
