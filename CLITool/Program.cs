using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Ookii.Dialogs.WinForms;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        // Store tasks
        List<Task> tasks = new List<Task>();
        
        // Ask the user to confirm
        var confirmation = AnsiConsole.Prompt(
            new TextPrompt<bool>("[yellow]Add a new task?[/]")
                .AddChoice(true)
                .AddChoice(false)
                .DefaultValue(true)
                .WithConverter(choice => choice ? "y" : "n"));
        if (!confirmation) return; // TODO: need to make check output for null then save output if valid
        // Echo the confirmation back to the terminal
        //Console.WriteLine(confirmation ? "Confirmed" : "Declined");
        
        // Ask the user for task details
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Task name"));
        var description = AnsiConsole.Prompt(
            new TextPrompt<string>("Task description"));
        var resources = AnsiConsole.Prompt(
            new TextPrompt<string>("Task description"));
        var dueDate = AnsiConsole.Prompt(
            new TextPrompt<string>("Task due date"));
        
        // Get the path to the users.json file in the same directory as the executable
        Dictionary<string, List<Dictionary<string, string>>> teams =
            new Dictionary<string, List<Dictionary<string, string>>>();
        string filePath = Path.Combine(AppContext.BaseDirectory, "Teams.json");
        try
        {
            // Read the JSON file content
            string json = File.ReadAllText(filePath);

            // Deserialize JSON into TeamData object
            teams = JsonConvert.DeserializeObject<
                Dictionary<string, List<Dictionary<string, string>>>>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            return;
        }
        
        // Ask the user for the assignees
        Dictionary<string, string> users = new Dictionary<string, string>();
        foreach (List<Dictionary<string, string>> roster in teams.Values)
        {
            foreach (Dictionary<string, string> user in roster) // for each user dict in in team list
            {
                // set this users data in the teamless users dict

                foreach (var kvp in user)
                {
                    users[kvp.Key] = kvp.Value;
                }
            }
        }
        List<string> realNames = new List<String>();
        List<string> options = new List<String>() { "individual" };
        foreach (var key in teams.Keys)
        {
            options.Add(key);
        }
        var assignees = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]team[/] to assign this task to?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more users)[/]")
                .AddChoices(options));
        if (assignees == "individual")
        {
            // Add all users to realNames
            foreach (var realName in users.Keys)
            {
                realNames.Add(realName);
            }
            
            // Ask for the users to include as assignees
            realNames = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select [green]users[/] to assign")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more users)[/]")
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle a user, " + 
                        "[green]<enter>[/] to confirm selected users)[/]")
                    .AddChoices(realNames));
        }
        else
        {
            // get members of that team
            foreach (Dictionary<string, string> user in teams[assignees])
            {
                // Add each username to real name
                foreach (string username in user.Keys)
                {
                    realNames.Add(username);
                }
            }
        }

        // Create task to add to master list
        tasks.Add(new Task()
        {
            Name = name,
            Description = description,
            Resources = resources,
            DueDate = dueDate,
            Users = new string[realNames.Count]
        });
        
        List<Text> userTexts = new List<Text>();
        for (int i = 0; i < realNames.Count; i++)
        {
            string username = users[realNames[i]];
            userTexts.Add(new Text(username));
            tasks[^1].Users[i] = username;
        }
        
        // Create the layout
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Left"),
                new Layout("Right")
                    .SplitRows(
                        new Layout("Top"),
                        new Layout("Bottom")));

        // Make task list UI
        //Panel tableContainer = new Panel(new Table("TEST"));

        // Update the left column
        //layout["Left"].Update();

        // Render the layout
        layout["Left"].MinimumSize(50);
        layout["Left"].Ratio(2);
        AnsiConsole.Write(layout);
    }

    // Generate CSV content from the list of Person objects
    static string GenerateCsv(List<Task> tasks)
    {
        using (var writer = new StringWriter())
        {
            writer.WriteLine("title,description,due_date,milestone");

            foreach (var task in tasks)
            {
                writer.WriteLine(task.ToString());
            }

            return writer.ToString();
        }
    }

    // Show file save dialog
    static string ShowSaveFileDialog()
    {
        using (var dialog = new VistaSaveFileDialog())
        {
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.DefaultExt = "csv";
            dialog.Title = "Save CSV File";
            
            return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.FileName : string.Empty;
        }
    }
}

// Simple Person class to hold data
class Task
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Resources { get; set; }
    public string DueDate { get; set; }
    public string[] Users { get; set; }

    public override string ToString()
    {
        return $"{Name},{DueDate},{Users},TEST";
    }
}
