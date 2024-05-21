#region Using Statements
using Jypeli;
using Jypeli.Assets;
using System;
#endregion

namespace Pallopeli
{
    /// @author Lauri Makkonen
    /// @version v1.0.0 (21.05.2024)
    /// <summary>
    /// Pidemmälle työstetty pallopeli. Tarkoituksena tuhota kaikki muut pallot.
    /// </summary>
    public class Pallopeli : PhysicsGame
    {
        PhysicsObject ylaReuna;
        PhysicsObject alaReuna;
        PhysicsObject vasenReuna;
        PhysicsObject oikeaReuna;
        PhysicsObject ohjattavaPallo;
        IntMeter pisteet;
        Vector nopeusYlos = new Vector(0.0, 200.0);
        Vector nopeusAlas = new Vector(0.0, -200.0);
        Vector nopeusVasem = new Vector(-200.0, 0.0);
        Vector nopeusOikea = new Vector(200.0, 0.0);


        public override void Begin()
        {
            LuoTaso();
            LuoPalloja(30);
            ohjattavaPallo = LuoPallo(40, Color.White, 0, -350);
            AddCollisionHandler(ohjattavaPallo, TormaysKasittelija);
            AsetaKontrollit();
            pisteet = LuoPisteLaskuri(Level.Left + 50, Level.Top - 50);
        }


        /// <summary>
        /// Luo satunnaisen värisen pallon, satunnaisella säteellä väliltä 5-30, satunnaisilla x- ja y-koordinaateilla.
        /// </summary>
        /// <returns>Luotu pallo.</returns>
        public PhysicsObject LuoPallo()
        {
            Random rnd = new Random();
            double randomSade = rnd.Next(5, 30);
            PhysicsObject pallo = new PhysicsObject(randomSade * 2, randomSade * 2, Shape.Circle, rnd.Next(-500, 500), rnd.Next(-350, 350));
            pallo.Color = RandomGen.NextColor();
            this.Add(pallo);
            return pallo;
        }


        /// <summary>
        /// Luo tietyn säteisen ja värisen pallon.
        /// </summary>
        /// <param name="sade"></param>
        /// <param name="vari"></param>
        /// <returns></returns>
        public PhysicsObject LuoPallo(double sade, Color vari, double x, double y)
        {
            Random rnd = new Random();
            PhysicsObject pallo = new PhysicsObject(sade * 2, sade * 2, Shape.Circle, x, y);
            pallo.Color = vari;
            this.Add(pallo);
            return pallo;
        }


        /// <summary>
        /// Luo n-määrän palloja.
        /// </summary>
        /// <param name="montakoLuodaan"></param>
        public void LuoPalloja(int montakoLuodaan)
        {
            for (int i = 0; i < montakoLuodaan; i++)
            {
                LuoPallo();
            }
            return;
        }


        /// <summary>
        /// Liikuttaa objektia vektorin suuntaan.
        /// </summary>
        /// <param name="objekti"></param>
        /// <param name="nopeus"></param>
        public void AsetaNopeus(PhysicsObject objekti, Vector nopeus)
        {
            objekti.Velocity = nopeus;
        }


        /// <summary>
        /// Alustaa pelin kaikki kontrollit.
        /// </summary>
        public void AsetaKontrollit()
        {
            /// ohjattavan pallon kontrollit
            Keyboard.Listen(Key.Up, ButtonState.Down, AsetaNopeus, null, ohjattavaPallo, nopeusYlos);
            Keyboard.Listen(Key.Down, ButtonState.Down, AsetaNopeus, null, ohjattavaPallo, nopeusAlas);
            Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, null, ohjattavaPallo, nopeusVasem);
            Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, null, ohjattavaPallo, nopeusOikea);
            Keyboard.Listen(Key.Up, ButtonState.Released, AsetaNopeus, null, ohjattavaPallo, Vector.Zero);
            Keyboard.Listen(Key.Down, ButtonState.Released, AsetaNopeus, null, ohjattavaPallo, Vector.Zero);
            Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, ohjattavaPallo, Vector.Zero);
            Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, ohjattavaPallo, Vector.Zero);

            /// yleiset kontrollit
            Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, null);
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, null);
        }


        /// <summary>
        /// Käsittelee törmäyksen.
        /// </summary>
        /// <param name="objekti"></param>
        /// <param name="kohde"></param>
        public void TormaysKasittelija(PhysicsObject objekti, PhysicsObject kohde)
        {
            if (kohde == ylaReuna || kohde == alaReuna || kohde == vasenReuna || kohde == oikeaReuna || kohde.IgnoresCollisionResponse) return;
            pisteet.AddValue(1);
            kohde.IgnoresCollisionResponse = true;
            objekti.FadeColorTo(kohde.Color, 0.5);

            Explosion rajahdys = new Explosion(50);
            rajahdys.ShockwaveColor = kohde.Color;
            rajahdys.X = kohde.X;
            rajahdys.Y = kohde.Y;
            rajahdys.Force = 0.01;
            rajahdys.Size = Vector.Zero;
            rajahdys.Volume = 0;
            this.Add(rajahdys);

            kohde.FadeColorTo(Color.Transparent, 0.5);
            objekti.Width += 0.5 * kohde.Width;
            objekti.Height += 0.5 * kohde.Height;



            /*
            Jypeli.Timer ajastin = new Jypeli.Timer(1000, Tuhoa(kohde));
            ajastin.TimesLimited = true;
            ajastin.Times.SetValue(1);
            ajastin.Enabled = true;
            */
            //        Timer ajastin = new Timer(LuoPisteLaskuri)



        }


        /// <summary>
        /// Alustaa kentän.
        /// </summary>
        public void LuoTaso()
        {
            Level.BackgroundColor = Color.Black;
            // StorageFile tiedosto;
            // tiedosto = new Image(tiedosto)
            //Level.Background.SetImage("D:\\Users\\Työ\\Downloads\\bgtest.jpg");
            ylaReuna = Level.CreateTopBorder(0.5, false);
            alaReuna = Level.CreateBottomBorder(0.5, false);
            vasenReuna = Level.CreateLeftBorder(0.5, false);
            oikeaReuna = Level.CreateRightBorder(0.5, false);
            Camera.ZoomToAllObjects(0);

        }


        public IntMeter LuoPisteLaskuri(double x, double y)
        {
            IntMeter laskuri = new IntMeter(0);
            Label naytto = new Label();
            naytto.BindTo(laskuri);
            naytto.X = x;
            naytto.Y = y;
            naytto.TextColor = Color.White;
            naytto.Font = Font.DefaultBold;
            this.Add(naytto);
            return laskuri;
        }


        public void Tuhoa(PhysicsObject kohde)
        {
            kohde.Destroy();
        }
    }
}
