using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Spectre.Console;
using System.Windows.Forms;
using Panel = Spectre.Console.Panel;
using Newtonsoft.Json;

class Program
{
    public enum UserAction
    {
        View_Tasks, 
        Add_New_Task, 
        Remove_Tasks, 
        Export_Tasks, 
        Clear_All_Tasks, 
        Quit
    }

    public static List<Task> tasks;
    static void Main(string[] args)
    {
        // Store tasks
        tasks = new List<Task>();
        
        // Start prompt loop
        var confirmation = GetUserAction();
        while (confirmation != UserAction.Quit)
        {
            switch (confirmation)
            {
                case UserAction.View_Tasks:
                    // Show task list
                    ShowTaskList(tasks);
                    Console.ReadKey();
                    break;
                
                case UserAction.Add_New_Task:
                    // Create new task to add to master list
                    Task t = PromptForTask();
                    tasks.Add(t);
                    break;
                
                case UserAction.Remove_Tasks:
                    // Early escape if no tasks to remove
                    if (tasks.Count == 0) break;
                    foreach (var task in PromptTaskMultiSelection(tasks))
                    {
                        tasks.Remove(task);
                    }
                    break;
                
                case UserAction.Export_Tasks:
                    // Early escape if no tasks to export
                    if (tasks.Count == 0) break;
                    string csv = GenerateCsv(PromptTaskMultiSelection(tasks));
                    ExportCSV(csv);
                    /*AnsiConsole.Clear();
                    AnsiConsole.Write(new Text(csv));
                    Console.ReadKey();*/
                    break;
                
                case UserAction.Clear_All_Tasks:
                    // Early escape if no tasks to clear
                    if (tasks.Count == 0) break;
                    var confirmed = AnsiConsole.Prompt(
                        new TextPrompt<bool>("Are you sure you want to clear " +
                                "all tasks? This action can not be undone.")
                            .AddChoice(true)
                            .AddChoice(false)
                            .DefaultValue(true)
                            .WithConverter(choice => choice ? "y" : "n")
                    );
                    if (confirmed) tasks = new List<Task>();
                    break;
                
                case UserAction.Quit:
                default: return;
            }
            
            // Get next action
            confirmation = GetUserAction();
        }
    }

    public static Task PromptTaskSelection(List<Task> tasks)
    {
        // Get options
        var options = new List<string>();
        foreach (var t in tasks)
        {
            options.Add($"{t.Name}: {t.DueDate}");
        }
                    
        // Display tasks as selection list
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select task to edit")
                .PageSize(10)
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more actions)[/]")
                .AddChoices(options)
        );
        
        // Search for and return selected in tasks
        Task task = new Task();
        foreach (var t in tasks)
        {
            if (selected.Contains(t.Name))
            {
                task = t;
                break;
            }
        }
        return task;
    }
    
    public static List<Task> PromptTaskMultiSelection(List<Task> tasks)
    {
        // Get options
        var options = new List<string>();
        foreach (var t in tasks)
        {
            options.Add($"{t.Name}: {t.DueDate}");
        }
                    
        // Display tasks as selection list
        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select task to edit")
                .PageSize(10)
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more actions)[/]")
                .AddChoices(options)
        );
        
        // Search for and return selected in tasks
        List<Task> selections = new List<Task>();
        foreach (var t in tasks)
        {
            foreach (var selection in selected)
            {
                if (selection.Contains(t.Name))
                {
                    selections.Add(t);
                    break;
                }
            }
        }
        return selections;
    }

    public static UserAction GetUserAction()
    {
        // Clear console
        AnsiConsole.Clear();
        
        // Get options as strings
        var options = new List<string>();
        foreach (var option in Enum.GetNames(typeof(UserAction)))
        {
            options.Add(option.Replace("_", " "));
        }
        
        // Prompt user action
        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select action")
                .PageSize(10)
                .MoreChoicesText(
                    "[grey](Move up and down to reveal more actions)[/]")
                .AddChoices(options)
        );
        
        // Get user selection as action
        action = action.Replace(" ", "_");
        return (UserAction)Enum.Parse(typeof(UserAction), action);
    }

    public static Layout GetTaskListElement()
    {
        // Create the layout
        var layout = new Layout("Root")
                .SplitColumns(
                    new Layout("Main"),
                    new Layout("Other").SplitColumns(
                        new Layout("Description"),
                        new Layout("Resources")
                    )
                );

        layout["Main"].Size(32);
        layout["Other"].MinimumSize(64);

        return layout;
    }

    public static void ShowTaskList(List<Task> tasks)
    {
        // Clear console
        AnsiConsole.Clear();
        
        // Early escape if no tasks
        if (tasks.Count == 0)
        {
            var tempUI = new Layout("Temp").SplitColumns(
                new Layout("LogoText"),
                new Layout("LogoArt").Size(32)
            );
            tempUI["LogoText"].Update(
                new FigletText("No Tasks Found")
                    .Centered()
                    .Color(Color.White)
            );
            tempUI["LogoArt"].Update(
                new Text("\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28e0\u28e4\u28e4\u28e4\u28e4\u28e4\u28f6\u28e6\u28e4\u28c4\u2840\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2880\u28f4\u28ff\u287f\u281b\u2809\u2819\u281b\u281b\u281b\u281b\u283b\u28bf\u28ff\u28f7\u28e4\u2840\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28fc\u28ff\u280b\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2880\u28c0\u28c0\u2808\u28bb\u28ff\u28ff\u2844\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28f8\u28ff\u284f\u2800\u2800\u2800\u28e0\u28f6\u28fe\u28ff\u28ff\u28ff\u283f\u283f\u283f\u28bf\u28ff\u28ff\u28ff\u28c4\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2801\u2800\u2800\u28b0\u28ff\u28ff\u28ef\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u2819\u28bf\u28f7\u2844\u2800 \n\u2800\u2800\u28c0\u28e4\u28f4\u28f6\u28f6\u28ff\u285f\u2800\u2800\u2800\u28b8\u28ff\u28ff\u28ff\u28c6\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28f7\u2800 \n\u2800\u28b0\u28ff\u285f\u280b\u2809\u28f9\u28ff\u2847\u2800\u2800\u2800\u2818\u28ff\u28ff\u28ff\u28ff\u28f7\u28e6\u28e4\u28e4\u28e4\u28f6\u28f6\u28f6\u28f6\u28ff\u28ff\u28ff\u2800 \n\u2800\u28b8\u28ff\u2847\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2839\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u287f\u2803\u2800 \n\u2800\u28f8\u28ff\u2847\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2809\u283b\u283f\u28ff\u28ff\u28ff\u28ff\u287f\u283f\u283f\u281b\u28bb\u28ff\u2847\u2800\u2800 \n\u2800\u28ff\u28ff\u2801\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28e7\u2800\u2800 \n\u2800\u28ff\u28ff\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28ff\u2800\u2800 \n\u2800\u28ff\u28ff\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28ff\u2800\u2800 \n\u2800\u28bf\u28ff\u2846\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u2847\u2800\u2800 \n\u2800\u2838\u28ff\u28e7\u2840\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2803\u2800\u2800 \n\u2800\u2800\u281b\u28bf\u28ff\u28ff\u28ff\u28ff\u28c7\u2800\u2800\u2800\u2800\u2800\u28f0\u28ff\u28ff\u28f7\u28f6\u28f6\u28f6\u28f6\u2836\u2800\u28a0\u28ff\u28ff\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u28fd\u28ff\u284f\u2801\u2800\u2800\u28b8\u28ff\u2847\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u28b9\u28ff\u2846\u2800\u2800\u2800\u28f8\u28ff\u2807\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28bf\u28ff\u28e6\u28c4\u28c0\u28e0\u28f4\u28ff\u28ff\u2801\u2800\u2808\u283b\u28ff\u28ff\u28ff\u28ff\u287f\u280f\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u281b\u283b\u283f\u283f\u283f\u283f\u280b\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n    ")
            );
            
            AnsiConsole.Write(tempUI);
            return;
        }
        
        // Create the layout
        var taskUIs = new Layout[tasks.Count + 1];
        taskUIs[0] = new Layout("Spacer").SplitRows(
            new Layout("Logo").SplitColumns(
                new Layout("LogoText"),
                new Layout("LogoArt").Size(32)),
            new Layout("Rule")
        );
        taskUIs[0]["LogoText"].Update(
            new FigletText("New Tasks List")
                .Centered()
                .Color(Color.White)
        );
        taskUIs[0]["LogoArt"].Update(
            new Text("\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28e0\u28e4\u28e4\u28e4\u28e4\u28e4\u28f6\u28e6\u28e4\u28c4\u2840\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2880\u28f4\u28ff\u287f\u281b\u2809\u2819\u281b\u281b\u281b\u281b\u283b\u28bf\u28ff\u28f7\u28e4\u2840\u2800\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28fc\u28ff\u280b\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2880\u28c0\u28c0\u2808\u28bb\u28ff\u28ff\u2844\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28f8\u28ff\u284f\u2800\u2800\u2800\u28e0\u28f6\u28fe\u28ff\u28ff\u28ff\u283f\u283f\u283f\u28bf\u28ff\u28ff\u28ff\u28c4\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2801\u2800\u2800\u28b0\u28ff\u28ff\u28ef\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u2819\u28bf\u28f7\u2844\u2800 \n\u2800\u2800\u28c0\u28e4\u28f4\u28f6\u28f6\u28ff\u285f\u2800\u2800\u2800\u28b8\u28ff\u28ff\u28ff\u28c6\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28f7\u2800 \n\u2800\u28b0\u28ff\u285f\u280b\u2809\u28f9\u28ff\u2847\u2800\u2800\u2800\u2818\u28ff\u28ff\u28ff\u28ff\u28f7\u28e6\u28e4\u28e4\u28e4\u28f6\u28f6\u28f6\u28f6\u28ff\u28ff\u28ff\u2800 \n\u2800\u28b8\u28ff\u2847\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2839\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u28ff\u287f\u2803\u2800 \n\u2800\u28f8\u28ff\u2847\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2809\u283b\u283f\u28ff\u28ff\u28ff\u28ff\u287f\u283f\u283f\u281b\u28bb\u28ff\u2847\u2800\u2800 \n\u2800\u28ff\u28ff\u2801\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28e7\u2800\u2800 \n\u2800\u28ff\u28ff\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28ff\u2800\u2800 \n\u2800\u28ff\u28ff\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u28ff\u2800\u2800 \n\u2800\u28bf\u28ff\u2846\u2800\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28b8\u28ff\u2847\u2800\u2800 \n\u2800\u2838\u28ff\u28e7\u2840\u2800\u28ff\u28ff\u2847\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2803\u2800\u2800 \n\u2800\u2800\u281b\u28bf\u28ff\u28ff\u28ff\u28ff\u28c7\u2800\u2800\u2800\u2800\u2800\u28f0\u28ff\u28ff\u28f7\u28f6\u28f6\u28f6\u28f6\u2836\u2800\u28a0\u28ff\u28ff\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u28fd\u28ff\u284f\u2801\u2800\u2800\u28b8\u28ff\u2847\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2800\u2800\u2800\u2800\u2800\u28ff\u28ff\u2847\u2800\u28b9\u28ff\u2846\u2800\u2800\u2800\u28f8\u28ff\u2807\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u28bf\u28ff\u28e6\u28c4\u28c0\u28e0\u28f4\u28ff\u28ff\u2801\u2800\u2808\u283b\u28ff\u28ff\u28ff\u28ff\u287f\u280f\u2800\u2800\u2800\u2800 \n\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2808\u281b\u283b\u283f\u283f\u283f\u283f\u280b\u2801\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\u2800\n    ")
        );
        taskUIs[0]["Rule"].Update(
            new Rule()
        );
        for (int i = 0; i < tasks.Count; i++)
        {
            taskUIs[i+1] = GetTaskListElement();
            taskUIs[i+1]["Main"].Update(
                new Rows(
                    new Markup(
                        $"Title: [bold]{tasks[i].Name}[/]", 
                        new Style(Color.DarkCyan, Color.Black)
                    ), 
                    new Markup(
                        $"DueDate: [bold]{tasks[i].DueDate}[/]", 
                        new Style(Color.DarkMagenta, Color.Black)
                    ), 
                    new Markup(
                        $"Labels: [bold]{String.Join("[/], [bold]", tasks[i].Labels)}[/]", 
                        new Style(Color.DarkGreen, Color.Black)
                    ), 
                    new Markup(
                        $"Users: [bold]{String.Join("[/], [bold]", tasks[i].Users)}[/]", 
                        new Style(System.ConsoleColor.DarkYellow, Color.Black)
                    )
                )
            );
            taskUIs[i+1]["Description"].Update(
                new Panel(
                    new Markup(tasks[i].Description)
                ).Expand()
            );
            taskUIs[i+1]["Resources"].Update(
                new Panel(
                    new Markup(tasks[i].Resources)
                ).Expand()
            );
        }
        
        // Add tasks to list
        var layout = new Layout("Root").SplitRows(taskUIs);

        // Render the layout
        AnsiConsole.Write(layout);
    }

    public static Task PromptForTask()
    {
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
            return new Task();
        }
        
        // Ask the user for task details
        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("Task name"));
        var description = AnsiConsole.Prompt(
            new TextPrompt<string>("Task description"));
        var resources = AnsiConsole.Prompt(
            new TextPrompt<string>("Task resources"));
        var dueDate = AnsiConsole.Prompt(
            new TextPrompt<string>("Task due date"));
        
        // Ask the user for the assignees in list of usernames
        Dictionary<string, string> users = new Dictionary<string, string>();
        List<string> usernames = new List<String>();
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
        List<string> options = new List<String>() { "individual users" };
        foreach (var key in teams.Keys)
        {
            options.Add(key);
        }
        var assignees = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select a [green]team[/] to assign this task to?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more users)[/]")
                .AddChoices(options)
        );
        if (assignees.Contains("individual"))
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
                    .AddChoices(realNames)
            );
        }
        else
        {
            // get members of that team
            foreach (Dictionary<string, string> user in teams[assignees])
            {
                // Add each username to real name
                foreach (string realName in user.Keys)
                {
                    realNames.Add(realName);
                }
            }
        }
        foreach (var realName in realNames)
        {
            usernames.Add(users[realName]);
        }
        
        // Get task labels
        List<string> labelSymbols = new List<String>();
        var labels = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select task [green]labels[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more labels)[/]")
                .InstructionsText(
                    "[grey](Press [blue]<space>[/] to toggle a label, " + 
                    "[green]<enter>[/] to confirm selection)[/]")
                .AddChoices(Enum.GetNames(typeof(Task.TaskType)))
        );
        foreach (var label in labels)
        {
            labelSymbols.Add($"~{label}");
        }

        // Create and return the new task
        return new Task()
        {
            Name = name,
            Description = description,
            Resources = resources,
            DueDate = dueDate,
            Labels = labelSymbols,
            Users = usernames
        };
    }
    
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
    
    static string ExportCSV(string csv)
    {
        string savePath = "";
        bool confirmed = false;
        while (savePath == "" && !confirmed)
        {
            savePath = AnsiConsole.Prompt(
                new TextPrompt<string>("Full save path (include [Blue].csv[/])")
                    .Validate((n) => 
                        n.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            );
            
            var pathUI = new TextPath(savePath).RightJustified();
            pathUI.RootStyle = new Style(foreground: Color.Green);
            pathUI.SeparatorStyle = new Style(foreground: Color.Red);
            pathUI.StemStyle = new Style(foreground: Color.Yellow);
            pathUI.LeafStyle = new Style(foreground: Color.Blue);
            AnsiConsole.Write(new Panel(pathUI));
            
            confirmed = AnsiConsole.Prompt(
                new TextPrompt<bool>("Confirm save path")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(choice => choice ? "y" : "n")
            );
        }
        
        // Delete the file if it exists.
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        
        //Create the file.
        using (FileStream fs = File.Create(savePath))
        {
            byte[] info = new UTF8Encoding(true).GetBytes(csv);
            fs.Write(info, 0, info.Length);
        }
        
        return savePath;
    }
}

class Task
{
    public enum TaskType { Bug, Document, Feature }
    
    public string Name { get; set; }
    public string Description { get; set; }
    public string Resources { get; set; }
    public string DueDate { get; set; }
    public List<string> Labels { get; set; }
    public List<string> Users { get; set; }
    
    public override string ToString()
    {
        return $"{Name},\"{Description}\n**RESOURCES**:{Resources}\n \n " +
               $"/assign {String.Join(", ", Users)}\n" +
               $"/label {String.Join(", ", Labels)}\",{DueDate},";
    }
}
