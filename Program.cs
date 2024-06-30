using System;
using System.Collections.Generic;

public interface IKonto
{
    int NumerKonta { get; }
    decimal Saldo { get; }

    void Wyplata(decimal kwota);
    void Wplata(decimal kwota);
    void Przelew(IKonto kontoDocelowe, decimal kwota);
}

public abstract class KontoBankowe : IKonto
{
    public int NumerKonta { get; }
    public decimal Saldo { get; protected set; }

    protected KontoBankowe(int numerKonta, decimal saldoPoczatkowe)
    {
        while (numerKonta.ToString().Length != 6)
        {
            Console.Write("Numer konta musi składać się z 6 cyfr. Podaj poprawny numer konta: ");
            numerKonta = int.Parse(Console.ReadLine());
        }

        if (saldoPoczatkowe < 0)
        {
            throw new ArgumentException("Saldo konta nie może być ujemne.");
        }

        NumerKonta = numerKonta;
        Saldo = saldoPoczatkowe;
    }

    public virtual void Wyplata(decimal kwota)
    {
        if (kwota <= 0 || kwota > Saldo)
        {
            throw new ArgumentException("Niepoprawna kwota wypłaty.");
        }

        Saldo -= kwota;
        Console.WriteLine($"Wypłacono: {kwota:F2} zł. Nowe saldo: {Saldo:F2} zł.");
    }

    public virtual void Wplata(decimal kwota)
    {
        if (kwota <= 0)
        {
            throw new ArgumentException("Niepoprawna kwota wpłaty.");
        }

        Saldo += kwota;
        Console.WriteLine($"\nWpłacono: {kwota:F2} zł. Nowe saldo: {Saldo:F2} zł.");
    }

    public abstract void Przelew(IKonto kontoDocelowe, decimal kwota);
}

public class OszczednoscioweKontoBankowe : KontoBankowe
{
    public OszczednoscioweKontoBankowe(int numerKonta, decimal saldoPoczatkowe)
        : base(numerKonta, saldoPoczatkowe)
    {
    }

    public override void Przelew(IKonto kontoDocelowe, decimal kwota)
    {
        if (kwota <= 0 || kwota > Saldo)
        {
            throw new ArgumentException("Niepoprawna kwota przelewu.");
        }

        if (Saldo - kwota < 0)
        {
            throw new ArgumentException("Niewystarczające środki na koncie źródłowym.");
        }

        Saldo -= kwota;
        kontoDocelowe.Wplata(kwota);
        Console.WriteLine($"Przelew: {kwota:F2} zł z konta {NumerKonta} na konto {kontoDocelowe.NumerKonta}.");
    }
}

public class Program
{
    static int WprowadzNumerKonta(List<int> numeryKont)
    {
        int numerKonta = 0;
        bool poprawnyNumerKonta = false;

        do
        {
            Console.Write("Wprowadź numer konta (6 cyfr): ");
            try
            {
                string input = Console.ReadLine();
                if (input.Length != 6 || !int.TryParse(input, out numerKonta) || numerKonta < 100000 || numerKonta > 999999)
                {
                    Console.WriteLine("Numer konta musi składać się z 6 cyfr w zakresie od 100000 do 999999.");
                }
                else if (numeryKont.Contains(numerKonta))
                {
                    Console.WriteLine("Konto o podanym numerze już istnieje. Wprowadź inny numer.");
                }
                else
                {
                    numeryKont.Add(numerKonta);
                    poprawnyNumerKonta = true;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Niepoprawny format numeru konta. Podaj numer składający się z cyfr.");
            }
        } while (!poprawnyNumerKonta);

        return numerKonta;
    }

    static decimal WprowadzSaldoPoczatkowe()
    {
        decimal saldoPoczatkowe = 0;
        bool poprawneSaldo = false;

        do
        {
            Console.Write("Wprowadź saldo początkowe: ");
            try
            {
                saldoPoczatkowe = decimal.Parse(Console.ReadLine());

                if (saldoPoczatkowe < 0)
                {
                    Console.WriteLine("Saldo początkowe nie może być ujemne. Podaj poprawne saldo.");
                }
                else
                {
                    poprawneSaldo = true;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Niepoprawny format salda. Podaj kwotę liczbową.");
            }
        } while (!poprawneSaldo);

        return saldoPoczatkowe;
    }

    static void Main(string[] args)
    {
        List<int> numeryKont = new List<int>();

        try
        {
            int numerKonta1 = WprowadzNumerKonta(numeryKont);
            decimal saldoPoczatkowe1 = WprowadzSaldoPoczatkowe();
            IKonto konto1 = new OszczednoscioweKontoBankowe(numerKonta1, saldoPoczatkowe1);

            Console.WriteLine("");

            int numerKonta2 = WprowadzNumerKonta(numeryKont);
            decimal saldoPoczatkowe2 = WprowadzSaldoPoczatkowe();
            IKonto konto2 = new OszczednoscioweKontoBankowe(numerKonta2, saldoPoczatkowe2);

            // Menu główne
            while (true)
            {
                Console.WriteLine("\nMenu główne:");
                Console.WriteLine("1. Wyplata");
                Console.WriteLine("2. Wplata");
                Console.WriteLine("3. Przelew");
                Console.WriteLine("4. Wyświetl salda");
                Console.WriteLine("0. Wyjście");

                Console.Write("\nWybierz opcję: ");
                int opcja = int.Parse(Console.ReadLine());

                switch (opcja)
                {
                    case 1:
                        IKonto wybraneKonto1 = WybierzKonto(new IKonto[] { konto1, konto2 });
                        do
                        {
                            Console.Write($"\nPodaj kwotę do wypłaty z konta {wybraneKonto1.NumerKonta}: ");
                            try
                            {
                                decimal kwotaWyplaty = decimal.Parse(Console.ReadLine());
                                wybraneKonto1.Wyplata(kwotaWyplaty);
                                break;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Niepoprawny format kwoty. Podaj kwotę liczbową.");
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine($"Błąd: {ex.Message} Podaj poprawną kwotę.");
                            }
                        } while (true);
                        break;
                    case 2:
                        IKonto wybraneKonto2 = WybierzKonto(new IKonto[] { konto1, konto2 });
                        do
                        {
                            Console.Write($"\nPodaj kwotę do wpłaty na konto {wybraneKonto2.NumerKonta}: ");
                            try
                            {
                                decimal kwotaWplaty = decimal.Parse(Console.ReadLine());
                                wybraneKonto2.Wplata(kwotaWplaty);
                                break;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Niepoprawny format kwoty. Podaj kwotę liczbową.");
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine($"Błąd: {ex.Message} Podaj poprawną kwotę.");
                            }
                        } while (true);
                        break;
                    case 3:
                        IKonto kontoZrodlowe = WybierzKonto(new IKonto[] { konto1, konto2 });
                        IKonto kontoDocelowe = WybierzKonto(new IKonto[] { konto1, konto2 });
                        do
                        {
                            Console.Write($"\nPodaj kwotę do przelewu z konta {kontoZrodlowe.NumerKonta} na konto {kontoDocelowe.NumerKonta}: ");
                            try
                            {
                                decimal kwotaPrzelewu = decimal.Parse(Console.ReadLine());
                                kontoZrodlowe.Przelew(kontoDocelowe, kwotaPrzelewu);
                                break;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine("Niepoprawny format kwoty. Podaj kwotę liczbową.");
                            }
                            catch (ArgumentException ex)
                            {
                                Console.WriteLine($"Błąd: {ex.Message} Podaj poprawną kwotę.");
                            }
                        } while (true);
                        break;
                    case 4:
                        Console.WriteLine("\nxxxxxxxxxxxxxxxxxxxxx");
                        Console.WriteLine($"Saldo konta {konto1.NumerKonta}: {konto1.Saldo:F2} zł");
                        Console.WriteLine("---------------------");
                        Console.WriteLine($"Saldo konta {konto2.NumerKonta}: {konto2.Saldo:F2} zł");
                        Console.WriteLine("xxxxxxxxxxxxxxxxxxxxx");
                        break;
                    case 0:
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("\nNiepoprawna opcja, spróbuj ponownie.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
    }

    static IKonto WybierzKonto(IKonto[] konta)
    {
        Console.WriteLine("\nDostępne konta:");
        for (int i = 0; i < konta.Length; i++)
        {
            Console.WriteLine($"{i + 1}. Numer konta: {konta[i].NumerKonta}, Saldo: {konta[i].Saldo:F2} zł");
        }

        int wybor = 0;
        bool poprawnyWybor = false;

        do
        {
            Console.Write("\nWybierz numer konta: ");
            try
            {
                wybor = int.Parse(Console.ReadLine());
                if (wybor < 1 || wybor > konta.Length)
                {
                    Console.WriteLine("Niepoprawny wybór konta. Podaj numer z zakresu dostępnych kont.");
                }
                else
                {
                    poprawnyWybor = true;
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Niepoprawny format wyboru. Podaj numer konta.");
            }
        } while (!poprawnyWybor);

        return konta[wybor - 1];
    }
}
