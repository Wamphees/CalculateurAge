using System.Collections.ObjectModel;
namespace CalculateurAge.ViewModels;

// Contient l'ÉTAT de l'écran et les ACTIONS possibles.
public class CalculateurViewModel : BaseViewModel
{
    // Champs privés : la vraie donnée.
    private string _nom = "";
    private string _statuts = "";
    private string _erreur = "";
    private DateTime _dateNaissance
        = DateTime.Today.AddYears(-20);
    private string _resultat = "";
    private bool _resultatVisible;
    private string _joursRestants = "";



    // Propriétés publiques : ce que le XAML voit.
    public string Nom
    {
        get => _nom;
        set
        {
            if (SetField(ref _nom, value))
                CalculerCommand.Rafraichir();
        }
    }

    
    public string Statuts
    {
        get => _statuts;
        set => SetField(ref _statuts, value);
    }
    public string Erreur
    {
        get => _erreur;
        set
        {
            if (SetField(ref _erreur, value))
                OnPropertyChanged(nameof(ErreurVisible));
        }
    }
    public string JoursRestants
    {
        get => _joursRestants;
        set => SetField(ref _joursRestants, value);
    }
    public bool ErreurVisible => !string.IsNullOrEmpty(Erreur);
    public RelayCommand EffacerCommand { get; }
    public string Resultat
    {
        get => _resultat;
        set => SetField(ref _resultat, value);
    }

    public bool ResultatVisible
    {
        get => _resultatVisible;
        set => SetField(ref _resultatVisible, value);
    }
    public ObservableCollection<string> Historique { get; } = new();
    // Lié à Button.Command dans le XAML.
    public RelayCommand CalculerCommand { get; }
   
   private void Effacer()
    {
        Nom = "";
        DateNaissance = DateTime.Today.AddYears(-20);
        Resultat = "";
        Statut = "";
        ResultatVisible = false;
    }
    private static DateTime ProchainAnniversaire(DateTime naissance)
    {
        DateTime Pour(int an) => new DateTime(an, naissance.Month,
            Math.Min(naissance.Day, DateTime.DaysInMonth(an, naissance.Month)));

        var prochain = Pour(DateTime.Today.Year);
        if (prochain < DateTime.Today) prochain = Pour(DateTime.Today.Year + 1);
        return prochain;
    }
    
    public CalculateurViewModel()
    {
        CalculerCommand = new RelayCommand(
            Calculer,
            () => !string.IsNullOrWhiteSpace(Nom)
                  && DateNaissance.Date <= DateTime.Today);

        EffacerCommand = new RelayCommand(Effacer);
    }

    // La logique métier : aucun contrôle d'interface ici.
    private void Calculer()
    {
        int age = DateTime.Today.Year
                  - DateNaissance.Year;
        if (DateNaissance.Date >
            DateTime.Today.AddYears(-age)) age--;

        Resultat = $"{Nom}, vous avez {age} ans";
        Historique.Insert(0, $"{Nom} : {age} ans");
        Statuts = age >= 18 ? "Majeur" : "Mineur";
        int jours = (ProchainAnniversaire(DateNaissance) - DateTime.Today).Days;
        JoursRestants = jours == 0
            ? "Joyeux anniversaire !"
            : $"Prochain anniversaire dans {jours} jour(s)";
        ResultatVisible = true;
    }
}