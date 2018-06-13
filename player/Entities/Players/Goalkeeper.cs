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
    public class Goalkeeper : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;

        public Goalkeeper(Team team, ICoach coach)
            : base(team, coach, true)
        {
            m_startPosition = new PointF(m_sideFactor * 30, 0);
        }

        private void MoveToGoaliePossition()
        {
            PointF goalPossition = GetGoalPossition("goal l");
            m_robot.Turn(138);
            bool KeepOnMoving = true;
            while (KeepOnMoving)
            {
                m_robot.Dash(80);
                SeenCoachObject myPossition = GetMyPossition();
                if (myPossition?.Pos.Value.X <= goalPossition.X + 7)
                {
                    KeepOnMoving = false;
                }
            }
        }
        private SeenCoachObject GetMyPossition()
        {
            SeenCoachObject seenCoachObject = m_coach.GetSeenCoachObject("player " + m_team.m_teamName + " " + m_number);
            return seenCoachObject;
        }
        private PointF GetGoalPossition(string Goal)
        {
            var myPossition = m_coach.GetSeenCoachObject(Goal);
            return (PointF)myPossition.Pos;
        }

        private PointF GetTopLimitPoint()
            {
            PointF topLimitPoint;
            if (m_side == 'l')
            {
                topLimitPoint = (PointF)FlagNameToPointF.Convert("flag p l t");   
            }
            else
            {
                topLimitPoint = (PointF)FlagNameToPointF.Convert("flag p r t");
            }
            return topLimitPoint;
        }

        private PointF GetBottomLimitPoint()
        {
            PointF bottomLimitPoint;
            if (m_side == 'l')
            {
                bottomLimitPoint = (PointF)FlagNameToPointF.Convert("flag p l b");
            }
            else
            {
                bottomLimitPoint = (PointF)FlagNameToPointF.Convert("flag p r b");
            }
            return bottomLimitPoint;
        }

        public override void play()
        {
            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);
            //MoveToGoaliePossition();
            PointF startPoint;
            if (m_side == 'l')
            {
                startPoint = (PointF)FlagNameToPointF.Convert("goal l");
                startPoint.X += 1;
            }
            else
            {
                startPoint = (PointF)FlagNameToPointF.Convert("goal r");
                startPoint.X -= 1;
            }
            while (!goToCoordinate(startPoint,1))
            {
                try
                {
                    Thread.Sleep(SoccerParams.simulator_step);
                }
                catch (Exception e)
                {

                }
            }
            TurnToAngle0();  // Turn to the opponent's goal.

            while (!m_timeOver)
            {
                SeenObject ball = null;
                SeenObject goal = null;

                //Get current player's info:
                var bodyInfo = GetBodyInfo();
                Console.WriteLine($"Kicks so far : {bodyInfo.Kick}");

                

                while (ball == null || ball.Distance > 1.5)
                {
                    //Get field information from god (coach).
                    var ballPosByCoach = m_coach.GetSeenCoachObject("ball");
                    if (ballPosByCoach != null && ballPosByCoach.Pos != null)
                    {
                        //Console.WriteLine($"Ball Position {ballPosByCoach.Pos.Value.X}, {ballPosByCoach.Pos.Value.Y}");
                    }

                    m_memory.waitForNewInfo();
                    ball = m_memory.GetSeenObject("ball");
                    if (ball == null)
                    {
                        // If you don't know where is ball then find it
                        m_robot.Turn(50);
                        m_memory.waitForNewInfo();
                    }
                    else if (ball.Distance > 1.5)
                    {
                        if (ballPosByCoach != null && ballPosByCoach.Pos != null)
                        {
                            // Check goal keeper is within field limit.
                            PointF topLimitPoint = GetTopLimitPoint();
                            PointF bottomLimitPoint = GetBottomLimitPoint();
                            // Run to ball coordibates.
                            bool reachedCoordinate = goToCoordinate(new PointF(ballPosByCoach.Pos.Value.X, ballPosByCoach.Pos.Value.Y),1, topLimitPoint, bottomLimitPoint);
                        }
                        else
                        {
                            // If ball is too far then
                            // turn to ball or 
                            // if we have correct direction then go to ball
                            if (Math.Abs((double)ball.Direction) < 0)
                                m_robot.Turn(ball.Direction.Value);
                            else
                                m_robot.Dash(10 * ball.Distance.Value);
                        }
                    }
                    else  // ball.Distance <= 1.5, so we can catch the ball.
                    {
                        m_robot.Catch(ball.Direction.Value);
                        Thread.Sleep(SoccerParams.simulator_step);
                        TurnToAngle0();
                        Thread.Sleep(SoccerParams.simulator_step);
                        m_robot.Move(-40, 15);
                        Thread.Sleep(SoccerParams.simulator_step);
                        double angleTo0 = GetAngleTo0();
                        m_robot.Kick(100, angleTo0);
                        Console.WriteLine($"Kick angleTo0: {angleTo0}");


                    }
                }









                // sleep one step to ensure that we will not send
                // two commands in one cycle.
                try
                {
                    Thread.Sleep(SoccerParams.simulator_step);
                }
                catch (Exception e)
                {

                }
            }  // DROR
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
