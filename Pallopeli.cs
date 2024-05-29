#region Using Statements
using FarseerPhysics.Dynamics;
using Jypeli;
using Jypeli.Assets;
using System;
#endregion

namespace Pallopeli
{
    /// @author Lauri Makkonen
    /// @version v1.3.0 (29.05.2024)
    /// <summary>
    /// Pidemmälle työstetty pallopeli. Tarkoituksena tuhota kaikki muut pallot.
    /// Ohjattava pallo kasvaa aina kun se saa uuden pallon tuhottua.
    /// On siis mahdollista ettei saa kaikkia palloja kentältä ja näin häviää pelin.
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
        readonly Image tausta = LoadImage("Starfield 1 - 1024x1024.png");


        /// <summary>
        /// Pääohjelma.
        /// </summary>
        public override void Begin()
        {
            LuoKentta();
            ohjattavaPallo = LuoPallo(40, Color.White, 0, Level.Bottom + 40);
            LuoPalloja(30);
            AsetaKontrollit();
            pisteet = LuoPisteLaskuri(Level.Left + 50, Level.Top - 50);
            AddCollisionHandler(ohjattavaPallo, TormaysKasittelija);
        }


        /// <summary>
        /// Luo satunnaisen värisen pallon, satunnaisella säteellä väliltä 5-30, satunnaisilla x- ja y-koordinaateilla.
        /// Ei saa osua ohjattavaan palloon. Jos osuu niin annetaan uusia x- ja y- koordinaatteja kunnes ei osu.
        /// </summary>
        /// <returns>Luotu pallo.</returns>
        public PhysicsObject LuoPallo()
        {
            double randomSade = RandomGen.NextDouble(5, 31);
            PhysicsObject pallo = new PhysicsObject(randomSade * 2, randomSade * 2, Shape.Circle, 0.0, 0.0); ;
            bool liianLahella;

            do
            {
                pallo.X = RandomGen.NextDouble(Level.Left + randomSade, Level.Right - randomSade);
                pallo.Y = RandomGen.NextDouble(Level.Bottom + randomSade, Level.Top - randomSade);
                
                liianLahella = (ohjattavaPallo.Position.Distance(pallo.Position)) < ((ohjattavaPallo.Width / 2.0 + pallo.Width / 2.0));
            } while (liianLahella);

            pallo.Color = RandomGen.NextColor();
            this.Add(pallo);
            return pallo;
        }


        /// <summary>
        /// Luo tietyn säteisen ja värisen pallon tiettyyn (x,y) kohtaan kenttää.
        /// </summary>
        /// <param name="sade"></param>
        /// <param name="vari"></param>
        /// <returns></returns>
        public PhysicsObject LuoPallo(double sade, Color vari, double x, double y)
        {
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
        public static void AsetaNopeus(PhysicsObject objekti, Vector nopeus)
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
            Timer tuhoamisAjastin = new Timer();
            tuhoamisAjastin.Interval = 0.6;
            tuhoamisAjastin.Timeout += delegate { Tuhoa(kohde); };
            tuhoamisAjastin.Start();
            
            pisteet.AddValue(1);
            // kohde.MaxVelocity = 400;
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

            objekti.Body.Size += 0.5 * kohde.Body.Size;
            tuhoamisAjastin.Stop();
        }


        /// <summary>
        /// Tuhoaa parametrina viedyn PhysicsObject-olion.
        /// </summary>
        /// <param name="kohde"></param>
        public void Tuhoa(PhysicsObject kohde)
        {
            kohde.Destroy();
        }


        /// <summary>
        /// Alustaa kentän.
        /// </summary>
        public void LuoKentta()
        {
            Level.Background.Image = tausta;
            Level.Background.ScaleToLevelFull();
            Level.BackgroundColor = Color.Black;
            bool isVisible = false;
            ylaReuna = Level.CreateTopBorder(1.0, isVisible);
            alaReuna = Level.CreateBottomBorder(1.0, isVisible);
            vasenReuna = Level.CreateLeftBorder(1.0, isVisible);
            oikeaReuna = Level.CreateRightBorder(1.0, isVisible);
            Camera.ZoomToAllObjects(-200);
        }


        /// <summary>
        /// Luo pistelaskurin.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
    }
}