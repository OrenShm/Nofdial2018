using RoboCup.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using RoboCup.Infrastructure;

namespace RoboCup
{
    public class AttackerExample : Player
    {
        private const int WAIT_FOR_MSG_TIME = 10;
        private SeenCoachObject goal = null;
        private SeenCoachObject ballByCoach = null;

        public AttackerExample(Team team, ICoach coach) 
            : base(team, coach)
        {
            m_startPosition = new PointF(m_sideFactor * 10, 0);
        }


        public override void play()
        {
            // first ,ove to start position
            m_robot.Move(m_startPosition.X, m_startPosition.Y);

            while (!m_timeOver)
            {
                try
                {
                    var ball = GetBallDetailsByCoach();
                    if (ball != null)
                    {
                        var distanceToBall = GetDistanceToPoint(ball.Pos.Value);

                        if (NearBoal(distanceToBall))
                        {
                            Kick(ball, distanceToBall);
                        }
                        else
                        {
                            goToCoordinate(ball.Pos.Value, DistFromBallToKick);
                        }
                    }
                }
                catch (Exception e)
                {

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
            }
        }

        private void Kick(SeenCoachObject ball, double distanceToBall)
        {
            SetGaol();

            var player = GetMyPlayerDetailsByCoach();

            var distanceToGate = GetDistanceToPoint(goal.Pos.Value);

            var directiontoGoal = GetAngleToPoint(goal.Pos.Value);
            var angleToGoal = GetAngleToPoint(ball.Pos.Value);

            if (distanceToGate < 18)
            {
                KickOrMoveWithBall(100, distanceToBall, directiontoGoal, ball);
            }
            else
            {
                KickOrMoveWithBall(30, distanceToBall, directiontoGoal, ball);

            }
        }

        private void KickOrMoveWithBall(double power, double distanceToBall, double directiontoGoal, SeenCoachObject ball)
        {
            if (NearBoal(distanceToBall))
            {

                m_robot.Kick(power, directiontoGoal);
            }
            else
            {
                goToCoordinate(ball.Pos.Value, DistFromBallToKick);
            }
        }

        private bool NearBoal(double distanceToBall)
        {
            if(Math.Abs(distanceToBall) <= DistFromBallToKick)
            {
                return true;
            }
            return false;
        }

        private void SetGaol()
        {
            string goalStr = (m_side == 'l') ? "goal r" : "goal l";
            while (goal == null)
            {
                goal = m_coach.GetSeenCoachObject(goalStr);
            }
        }

        private bool TurnToGoal()
        {
            string goalStr = (m_side == 'l') ? "goal r" : "goal l";
            var coachGoal = m_coach.GetSeenCoachObject(goalStr);
            var playerPosByCoach = m_coach.GetSeenCoachObject($"player Yossi {m_number}");

            if (coachGoal != null && playerPosByCoach!= null && ballByCoach!=null)
            {
                if(coachGoal.Pos.Value.X < playerPosByCoach.Pos.Value.X && playerPosByCoach.Pos.Value.X  < ballByCoach.Pos.Value.X)
                {
                    if (goalStr.Equals("goal l"))
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (goalStr.Equals("goal r"))
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        //        public override void Play
        //        {

        //            // first ,ove to start position
        //            m_robot.Move(m_startPosition.X, m_startPosition.Y);

        //            SeenObject obj;

        //            while (!m_timeOver)
        //            {
        //                var bodyInfo = GetBodyInfo();

        //        obj = m_memory.GetSeenObject("ball");
        //                if (obj == null)
        //                {
        //                    // If you don't know where is ball then find it
        //                    m_robot.Turn(40);
        //                    m_memory.waitForNewInfo();
        //                }
        //                else if (obj.Distance.Value > 1.5)
        //                {
        //                    // If ball is too far then
        //                    // turn to ball or 
        //                    // if we have correct direction then go to ball
        //                    if (obj.Direction.Value != 0)
        //                        m_robot.Turn(obj.Direction.Value);
        //                    else
        //                        m_robot.Dash(10 * obj.Distance.Value);
        //}
        //                else
        //                {
        //                    // We know where is ball and we can kick it
        //                    // so look for goal
        //                    if (m_side == 'l')
        //                        obj = m_memory.GetSeenObject("goal r");
        //                    else
        //                        obj = m_memory.GetSeenObject("goal l");

        //                    if (obj == null)
        //                    {
        //                        m_robot.Turn(40);
        //                        m_memory.waitForNewInfo();
        //                    }
        //                    else
        //                        m_robot.Kick(100, obj.Direction.Value);
        //                }

        //                // sleep one step to ensure that we will not send
        //                // two commands in one cycle.
        //                try
        //                {
        //                    Thread.Sleep(2 * SoccerParams.simulator_step);
        //                }
        //                catch (Exception e) 
        //                {

        //                }
        //            }
        //        }
        //        }

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
