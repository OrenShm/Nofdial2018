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
        const double Rad2Deg = 180.0 / Math.PI;
        const double Deg2Rad = Math.PI / 180.0;
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



        public double GetAngleToPoint(PointF targetPoint)
        {
            Console.WriteLine("----------------------------------------");

            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var angleToTarget = Calc2PointsAngleByXAxis(myPosByCoach.Pos.Value, targetPoint);
            var myAbsAngle = myPosByCoach.BodyAngle;

            Console.WriteLine($"myAbsAngle: {myAbsAngle}");
            Console.WriteLine($"angleToTarget: {angleToTarget}");

            var turnAngle = -1 * (Convert.ToDouble(myAbsAngle) + angleToTarget);
            Console.WriteLine($"turnAngle: {turnAngle}");

            var fixedAngle = NormalizeTo180(turnAngle);
            Console.WriteLine($"fixedAngle: {fixedAngle}");


            return turnAngle;
        }

        //TODO: Check!
        public double AngelTo0()
        {
            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var myAbsAngle = myPosByCoach.BodyAngle;
            myAbsAngle = -1 * myAbsAngle;
            return NormalizeTo180(Convert.ToDouble(myAbsAngle));
        }


        //--------------------------Higher Level API-------------------------
        public void TurnToAngle0()
        {
            m_robot.Turn(AngelTo0());
        }


        /// <summary>
        /// Goes to requested coordinate
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns> true in case we reached the requested pos, false otherwise</returns>
        public bool goToCoordinate(PointF point)
        {
            var myPlayerDetails = GetMyPlayerDetailsByCoach();

            var dist = Math.Sqrt(Math.Pow(point.X - myPlayerDetails.Pos.Value.X, 2) + Math.Pow(point.Y - myPlayerDetails.Pos.Value.Y, 2));
            if (dist < 1)
            {
                return true;
            }
            var turnAngle = GetAngleToPoint(point);
            if (Math.Abs(turnAngle) > 10)
            {
                m_robot.Turn(turnAngle);
            }
            else
            {
                m_robot.Dash(Math.Min(100, 20 * dist));
            }
            return false;

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
