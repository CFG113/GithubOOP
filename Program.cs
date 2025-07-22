using System.Security.Cryptography;
using System.Text;

// Git has 3 states: modified, staged, committed

// Working tree: project files I'm editing
// Staging area: where changed files are staged for the next commit
// Git directory: where snapshots are stored locally (.git)

// git add: moves the modified files from the working tree to the staging area.
// git commit: takes the files from the staging area and stores that snapshot to the Git directory

// Each file in the working tree is in one of two states: Tracked or Untracked
// Tracked files are files Git is already tracking.
// Untracked files are new files that Git has not seen yet.

// Create and run Git clone logic
var git = new GithubClone(new[] { "file1.txt", "file2.txt" });
git.CreateNewFile("new.txt");
git.Modify("file1.txt");
git.Add(new[] { "file1.txt" });
git.Commit("Initial commit");

public class GithubClone
{
    private readonly Dictionary<string, string> _fileStates = new(); // tracked/untracked, modified, staged
    private readonly Dictionary<string, string> _stagingArea = new(); // file, hashed file
    private readonly Dictionary<string, string> _committedFiles = new();

    
    // DIRECTORIES
    // Base directory: the working tree
    // Git repository or metadata directory(.git): information git stores about ur files
    // .git/index: staging area holds a list of staged files where each file maps its relative path to its hashed content.
    // .git/Objects dir: stores the hashed content of each file (blobs, trees and commits)
    
    // Git Objects are stored by SHA-1 hash: first 2 characters as folder, remaining as filename
    
    // .git/Objects/blob dir: 'git add' hashes each file individually and stores it as a blob — e.g. 7c8763fc -> .git/objects/7c/8763fc
    // Tree: a directory listing (of blobs and trees).
    // .git/Objects/commits dir: stores each commit - commit hash, author, message, snapshot of all tracked files from the staging area
    
    // Git commits still store unchanged files if they are tracked
    // We can further add tree objects to snapshot the full tracked directory at each commit

    public GithubClone(string[] trackedFiles)
    {
        foreach (var file in trackedFiles)
            _fileStates.TryAdd(file, "tracked");
    }

    //add new file to working tree
    public void CreateNewFile(string file)
    {
        _fileStates.TryAdd(file, "untracked");
    }

    // Modifying a staged file doesn't unstage it git keeps both versions
    public void Modify(string file)
    {
        // check if there's no file or if the value is untracked return;
        if (!_fileStates.TryGetValue(file, out var state) || state == "untracked")
            return;
        
        //update file state to modified
        _fileStates[file] = "modified";
    }
    
    // add current files from modified to stage
    public void Add(string[] files)
    {
        foreach (var file in files)
        {
            if (!_fileStates.TryGetValue(file, out var state) || state != "modified")
                continue;

            // Hash the file content but for now just the file name for oop
            var bytes = Encoding.UTF8.GetBytes(file);
            var hashBytes = SHA1.HashData(bytes);
            var blobHash = Convert.ToBase64String(hashBytes);
            
            _stagingArea[file] = blobHash;
            _fileStates[file] = "staged";
        }
    }
    
    public void Commit(string message)
    {
        foreach (var file in _stagingArea.Keys)
        {
            var hash = _stagingArea[file];
            _committedFiles[file] = hash;
            
            // reset file state and remove the files from staging area
            _fileStates[file] = "tracked";
            _stagingArea.Clear();
        }
    }
}




