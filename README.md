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

