using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboCup.Interfaces
{
    public interface HighLevelCommands : ISendCommand
    {
        /// <summary>
        /// <param name="x">-52.5 .. 52.5</param>
        /// <param name="y">-34 .. 34</param>
        /// </summary>
        void DashToPossition(double x, double y);

        void Pass(double x, double y);
    }
}
