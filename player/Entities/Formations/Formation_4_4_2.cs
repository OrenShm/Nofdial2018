﻿using RoboCup.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup
{
    public class Formation_4_4_2 : IFormation
    {
        public List<Player> InitTeam(Team team, ICoach coach)
        {
            var players = new List<Player>();
            players.Add(new Goalkeeper(team, coach));
            //players.Add(new Dummy(team, coach));

            return players;
        }
    }
}
