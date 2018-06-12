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

        public Player(Team team, ICoach coach)
        {
            m_coach = coach;
            m_memory = new Memory();
            m_team = team;
            m_robot = new Robot(m_memory);
            m_robot.Init(team.m_teamName, out m_side, out m_number, out m_playMode);

            Console.WriteLine("New Player - Team: " + m_team.m_teamName + " Side:" + m_side +" Num:" + m_number);

            m_strategy = new Thread(new ThreadStart(play));
            m_strategy.Start();
        }

        public virtual  void play()
        {
 
        }

        public SeenCoachObject GetMyPlayerDetailsByCoach()
        {
            var res =  m_coach.GetSeenCoachObject($"player {m_team.m_teamName} {m_number}");
            if (res == null)
            {
                throw new Exception("Couldn't find my player");
            }
            return res;
        }

        private double Calc2PointsAngleByXAxis(PointF start, PointF end)
        {
            return Math.Atan2(start.Y - end.Y, end.X - start.X) * Rad2Deg;
        }

        public double GetAngleToPoint(PointF targetPoint)
        {
            Console.WriteLine("----------------------------------------");

            var myPosByCoach = GetMyPlayerDetailsByCoach();
            var angleToTarget = Calc2PointsAngleByXAxis(myPosByCoach.Pos.Value, targetPoint);
            var myAbsAngle = myPosByCoach.BodyAngle;

            Console.WriteLine($"myGolbalAngle: {myAbsAngle}");
            Console.WriteLine($"angleToTarget: {angleToTarget}");

            var turnAngle = -1*(Convert.ToDouble(myAbsAngle) + angleToTarget) % 360;
            Console.WriteLine($"turnAngle: {turnAngle}");

            var fixedAngle = turnAngle;

            if (Math.Abs(fixedAngle) > 180)
            {
                if (fixedAngle > 180)
                {
                    fixedAngle = fixedAngle - 360;
                }
                else
                {
                    fixedAngle = fixedAngle + 360;
                }
            }
            Console.WriteLine($"fixedAngle: {fixedAngle}");


            return turnAngle;
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
                m_robot.Dash(Math.Max(100, 20 * dist));
            }
            return false;

        }



    }
}
