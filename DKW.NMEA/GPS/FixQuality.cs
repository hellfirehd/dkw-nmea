/*
DKW.NMEA
Copyright (C) 2018 Doug Wilson

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace DKW.NMEA.GPS
{
    using System;

    public enum FixQuality : Int32
    {
        Invalid = 0,
        GpsFix = 1,
        DgpsFix = 2,
        PpsFix = 3,
        Rtk = 4,
        FloatRtk = 5,
        Estimated = 6,
        ManualInput = 7,
        Simulation = 8
    }
}
