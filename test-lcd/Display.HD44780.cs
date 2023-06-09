﻿/*
 * Driver for paralell LCD display 4 rows / 20 col type ECM2004D/PCM2004D
 * Driven in 4bits
 * written from the driver provided by GHI for the FEZ_shields LCD 2*16
 * 
 * --------------------API-------------------------
 * Initialize(), Putc(), SendCmd(), Print(), Clear(), CursorHome(),
 * SetCursor(), TurnBacklightOn(), TurnBacklightOff()
 * ------------------------------------------------
 * 
 * Author : GHI Electronics modified by Gregory Lepinard
*/

using System;
using System.Collections;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.Gpio;

namespace GHIElectronics.TinyCLR.Display.HD44780
{
    /* HD44780 pinnig with gadgeteer adapter
             * pin1 : 3.3V
             * pin2 : 5V
             * pin3 : E
             * pin4 : RS
             * pin5 : D4
             * pin6 : D7
             * pin7 : D5
             * pin8 : Backlight
             * pin9 : D6
             * pin10 : GND
             */

    class DisplayHD44780
    {
        private const byte DISP_ON = 0xC;    //Turn visible LCD on
        private const byte CLR_DISP = 1;      //Clear display
        private const byte CUR_HOME = 2;      //Move cursor home and clear screen memory
        private const byte SET_CURSOR = 0x80;   //SET_CURSOR + X : Sets cursor position to X
        private readonly GpioPin[] D4;
        private readonly GpioPin E, RS;

        public DisplayHD44780(GpioPin[] D4, GpioPin E, GpioPin RS)
        {
            this.D4 = D4;
            this.E = E;
            this.RS = RS;
            foreach (var pin in this.D4)
            {
                pin.SetDriveMode(GpioPinDriveMode.Output);
                pin.Write(GpioPinValue.Low);
            }
            this.E.SetDriveMode(GpioPinDriveMode.Output);
            this.E.Write(GpioPinValue.Low);
            this.RS.SetDriveMode(GpioPinDriveMode.Output);
            this.RS.Write(GpioPinValue.Low);

            // 4 bit data communication
            Thread.Sleep(50);

            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0011);
            Thread.Sleep(50);
            this.WriteNibble(0b0010);
            this.SendCommand(DISP_ON);
            this.SendCommand(CLR_DISP);
        }

        private void WriteNibble(int c)
        {
            this.D4[3].Write(((c & 0x8) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.D4[2].Write(((c & 0x4) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.D4[1].Write(((c & 0x2) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.D4[0].Write(((c & 0x1) != 0) ? GpioPinValue.High : GpioPinValue.Low);
            this.E.Write(GpioPinValue.High); this.E.Write(GpioPinValue.Low);

        }

        private void SendData(int c)
        {
            this.RS.Write(GpioPinValue.High); //set LCD to data mode
            this.WriteNibble(c >> 4);
            this.WriteNibble(c);
            Thread.Sleep(1);
        }

        private void SendCommand(int c)
        {
            this.RS.Write(GpioPinValue.Low);
            this.WriteNibble(c >> 4);
            this.WriteNibble(c);
            Thread.Sleep(1);

        }

        public void Print(string str)
        {
            //foreach (var c in str)
            //  this.SendData(c);
            for (var i = 0; i < str.Length; i++)
                this.SendData(str[i]);
        }

        public void Clear() => this.SendCommand(CLR_DISP);

        public void CursorHome() => this.SendCommand(CUR_HOME);

        // to use if only 2 lines displays
        //public void SetCursor(byte row, byte col) => this.SendCommand(SET_CURSOR | row << 6 | col);

        // to use if only 4 lines displays
        public void SetCursor(int raw, int col)
        {
            switch (raw)
            {
                case 1:
                    this.SendCommand((byte)((0x80 + col) - 1));  // Line 1
                    break;
                case 2:
                    this.SendCommand((byte)((0xC0 + col) - 1));  // Line 2
                    break;
                case 3:
                    this.SendCommand((byte)((0x94 + col) - 1));  // Line 3
                    break;
                case 4:
                    this.SendCommand((byte)((0xD4 + col) - 1));  // Line 4
                    break;
            }
        }
    }
}
