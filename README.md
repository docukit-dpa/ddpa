# Docukit Data Protection App

ICONZ-Webvisions Pte Ltd, in partnership with docukit, developed this tool as a result of the Open Innovation Platform’s Personal Data Asset Inventory Tool Challenge that is facilitated by the Personal Data Protection Commission Singapore.

You can use this free tool to map and keep track of how personal data is being managed within their organisation and across all data touchpoints in a consistent manner, as well as monitor the remediation progress for the gaps and issues identified.

## Getting Started

After cloning the Project you should be able to run it in your machine with the following prerequisites installed.

### Prerequisites
- .Net Core version 2.1.0([download link here](https://dotnet.microsoft.com/download/dotnet-core/2.1))
- MySQL version 8.0.15.0([download link here](https://downloads.mysql.com/archives/installer/))

### Installer
- docukit DP App([download link here](https://www.dataprotect.sg/data-protect-download/))

### Running
After installing the prerequisites, follow the steps below for the Project to run in your machine.

1. Update the `appsettings.Development.json`'s `ConnectionStrings.DefaultConnection`base on your MySQL's installation input.
```
    {
        "ConnectionStrings": {
            "DefaultConnection": "Server=localhost;Port=3306;Database=ddpa;UserID=root;Password=root;"
    },
```
2. Clean and Rebuild the solution.
3. Run the application.
   - In initial running of the application, it will create and seed the database. This process usually takes one to two minutes.
   - If the seeding is interrupted in anyway, you will need to delete the created database in MySQL and re-run the application.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details
