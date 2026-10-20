using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lab3
{
    // exercice 1
    // an interface cannot store an attribute, only declare the property.
    // the _name attribute is therefore in the implementing classes.
    // if a method is needed by IMammal and IReptile, it is declared in IAnimal
    // (otherwise a class implementing both would inherit it twice).
    public interface IAnimal
    {
        string name { get; set; }
        void move();
        void eat();
    }

    public interface IMammal : IAnimal
    {
        void suckle();
    }

    public interface IReptile : IAnimal
    {
        void moult();
    }

    [Serializable]
    public class Lion : IMammal
    {
        private string _name;
        public Lion(string n) { _name = n; }
        public string name { get { return _name; } set { _name = value; } }
        public void move() { Console.WriteLine(_name + " the lion runs"); }
        public void eat() { Console.WriteLine(_name + " the lion eats meat"); }
        public void suckle() { Console.WriteLine(_name + " suckles its cubs"); }
    }

    [Serializable]
    public class Cow : IMammal
    {
        private string _name;
        public Cow(string n) { _name = n; }
        public string name { get { return _name; } set { _name = value; } }
        public void move() { Console.WriteLine(_name + " the cow walks"); }
        public void eat() { Console.WriteLine(_name + " the cow eats grass"); }
        public void suckle() { Console.WriteLine(_name + " suckles its calf"); }
    }

    [Serializable]
    public class Snake : IReptile
    {
        private string _name;
        public Snake(string n) { _name = n; }
        public string name { get { return _name; } set { _name = value; } }
        public void move() { Console.WriteLine(_name + " the snake crawls"); }
        public void eat() { Console.WriteLine(_name + " the snake swallows a mouse"); }
        public void moult() { Console.WriteLine(_name + " sheds its skin"); }
    }

    [Serializable]
    public class Lizard : IReptile
    {
        private string _name;
        public Lizard(string n) { _name = n; }
        public string name { get { return _name; } set { _name = value; } }
        public void move() { Console.WriteLine(_name + " the lizard runs on the wall"); }
        public void eat() { Console.WriteLine(_name + " the lizard eats insects"); }
        public void moult() { Console.WriteLine(_name + " sheds its skin"); }
    }

    // multiple interfaces are allowed, so a platypus can be both
    [Serializable]
    public class Platypus : IMammal, IReptile
    {
        private string _name;
        public Platypus(string n) { _name = n; }
        public string name { get { return _name; } set { _name = value; } }
        public void move() { Console.WriteLine(_name + " the platypus swims"); }
        public void eat() { Console.WriteLine(_name + " the platypus eats worms"); }
        public void suckle() { Console.WriteLine(_name + " suckles its young"); }
        public void moult() { Console.WriteLine(_name + " changes its fur"); }
    }

    // exercice 2
    [Serializable]
    public class Zoo
    {
        private List<IAnimal> _animals = new List<IAnimal>();

        public void Add(IAnimal a)
        {
            _animals.Add(a);
        }

        public void WalkAll()
        {
            foreach (IAnimal a in _animals) a.move();
        }

        public void EatAll()
        {
            foreach (IAnimal a in _animals) a.eat();
        }

        public void Serialize(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, _animals);
            fs.Close();
        }

        public void Deserialize(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            _animals = (List<IAnimal>)bf.Deserialize(fs);
            fs.Close();
        }
    }

    // exercice 3
    // with normal classes and no virtual/override, the call is resolved at compile
    // time on the declared type : an Animal reference calls Animal.move(), the
    // subclass version is only hidden (new), not overridden.
    // with interfaces the call was always dynamic, so the behaviour is different.
    // to get the same behaviour : declare the methods virtual in Animal and use
    // override (not new) in the subclasses.
    [Serializable]
    public class Animal
    {
        protected string _name;

        public Animal(string n) { _name = n; }

        public string name { get { return _name; } set { _name = value; } }

        public virtual void move() { Console.WriteLine(_name + " : Animal is moving"); }
        public virtual void eat() { Console.WriteLine(_name + " : Animal is eating"); }
    }

    [Serializable]
    public class Mammal : Animal
    {
        public Mammal(string n) : base(n) { }
        public override void move() { Console.WriteLine(_name + " : Mammal is walking"); }
        public override void eat() { Console.WriteLine(_name + " : Mammal is chewing"); }
        public virtual void suckle() { Console.WriteLine(_name + " suckles"); }
    }

    [Serializable]
    public class Reptile : Animal
    {
        public Reptile(string n) : base(n) { }
        public override void move() { Console.WriteLine(_name + " : Reptile is crawling"); }
        public override void eat() { Console.WriteLine(_name + " : Reptile is swallowing"); }
        public virtual void moult() { Console.WriteLine(_name + " moults"); }
    }

    [Serializable]
    public class Lion2 : Mammal
    {
        public Lion2(string n) : base(n) { }
        public override void move() { Console.WriteLine(_name + " the lion runs"); }
        public override void eat() { Console.WriteLine(_name + " the lion eats meat"); }
    }

    [Serializable]
    public class Snake2 : Reptile
    {
        public Snake2(string n) : base(n) { }
        public override void move() { Console.WriteLine(_name + " the snake crawls"); }
        public override void eat() { Console.WriteLine(_name + " the snake swallows a mouse"); }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Zoo z = new Zoo();
            z.Add(new Lion("Leo"));
            z.Add(new Cow("Marguerite"));
            z.Add(new Snake("Kaa"));
            z.Add(new Lizard("Liz"));
            z.Add(new Platypus("Perry"));

            z.WalkAll();
            z.EatAll();

            z.Serialize("zoo.bin");

            Zoo z2 = new Zoo();
            z2.Deserialize("zoo.bin");
            Console.WriteLine("after deserialization :");
            z2.WalkAll();

            // exercice 3 : same test with classes instead of interfaces
            Animal a1 = new Lion2("Leo");
            Animal a2 = new Snake2("Kaa");
            a1.move();
            a2.move();
        }
    }
}
