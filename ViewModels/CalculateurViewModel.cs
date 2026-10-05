using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace CalculateurAge.ViewModels;

// Contient l'ÉTAT de l'écran et les ACTIONS possibles.
public class CalculateurViewModel : BaseViewModel
{
    private const string HistoriquePreferenceKey = "HistoriqueCalculateurAge";

    // Champs privés : la vraie donnée.
    private string _nom = "";
    private string _statut = "";
    private string _erreur = "";
    private string _erreurHistorique = "";
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

    public DateTime DateNaissance
    {
        get => _dateNaissance;
        set
        {
            if (SetField(ref _dateNaissance, value))
            {
                Erreur = value.Date > DateTime.Today
                    ? "La date de naissance ne peut pas être dans le futur"
                    : "";
                CalculerCommand.Rafraichir();
            }
        }
    }
    
    public string Statut
    {
        get => _statut;
        set => SetField(ref _statut, value);
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

    public string ErreurHistorique
    {
        get => _erreurHistorique;
        set
        {
            if (SetField(ref _erreurHistorique, value))
                OnPropertyChanged(nameof(ErreurHistoriqueVisible));
        }
    }

    public string JoursRestants
    {
        get => _joursRestants;
        set => SetField(ref _joursRestants, value);
    }

    public bool ErreurVisible => !string.IsNullOrEmpty(Erreur);
    public RelayCommand EffacerCommand { get; }
    public RelayCommand ViderHistoriqueCommand { get; }

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

    public bool ErreurHistoriqueVisible =>
        !string.IsNullOrEmpty(ErreurHistorique);

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

    private void ViderHistorique()
    {
        Historique.Clear();
        Preferences.Default.Remove(HistoriquePreferenceKey);
        ErreurHistorique = "";
        ViderHistoriqueCommand.Rafraichir();
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
        ChargerHistorique();

        CalculerCommand = new RelayCommand(
            Calculer,
            () => !string.IsNullOrWhiteSpace(Nom)
                  && DateNaissance.Date <= DateTime.Today);

        EffacerCommand = new RelayCommand(Effacer);
        ViderHistoriqueCommand = new RelayCommand(
            ViderHistorique,
            () => Historique.Count > 0 || ErreurHistoriqueVisible);
        Historique.CollectionChanged += (_, _) =>
            ViderHistoriqueCommand.Rafraichir();
    }

    private void ChargerHistorique()
    {
        var historiqueSauvegarde = Preferences.Default.Get(
            HistoriquePreferenceKey,
            string.Empty);
        if (string.IsNullOrWhiteSpace(historiqueSauvegarde))
            return;

        try
        {
            var lignes = JsonSerializer.Deserialize<List<string>>(
                historiqueSauvegarde)
                ?? throw new JsonException("L'historique est vide.");

            foreach (var ligne in lignes)
                Historique.Add(ligne);
        }
        catch (JsonException)
        {
            ErreurHistorique =
                "L'historique enregistré est illisible. Vous pouvez le vider.";
        }
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
        Preferences.Default.Set(
            HistoriquePreferenceKey,
            JsonSerializer.Serialize(Historique));
        ErreurHistorique = "";
        Statut = age >= 18 ? "Majeur" : "Mineur";
        int jours = (ProchainAnniversaire(DateNaissance) - DateTime.Today).Days;
        JoursRestants = jours == 0
            ? "Joyeux anniversaire !"
            : $"Prochain anniversaire dans {jours} jour(s)";
        ResultatVisible = true;
    }
}