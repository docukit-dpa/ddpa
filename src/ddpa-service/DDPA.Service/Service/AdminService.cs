using Microsoft.Extensions.Logging;
using DDPA.Commons.Results;
using System.Threading.Tasks;
using DDPA.DTO;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using System;
using static DDPA.Commons.Enums.DDPAEnums;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DDPA.Service.Extensions;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Http;
using DDPA.Commons.Settings;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.Text;
using CsvHelper;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace DDPA.Service
{
    public class AdminService :  IAdminService
    {
        protected readonly ILogger _logger;
        protected readonly IRepository _repo;
        private readonly IMapper _mapper;
        private readonly IQueryService _queryService;
        private readonly IValidationService _validationService;
        private readonly IOptions<DDPAOptions> _ddpaOptions;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AdminService(ILogger<AccountService> logger, IRepository repo, IQueryService queryService, IOptions<DDPAOptions> ddpaOptions, IValidationService validationService, IHostingEnvironment hostingEnvironment) 
        {
            _logger = logger;
            _repo = repo;
            _queryService = queryService;
            _ddpaOptions = ddpaOptions;
            _validationService = validationService;
            _mapper = this.GetMapper();
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<Result> AddFields(List<AddFieldDTO> dto, string userId)
        {
            var response = new Result();
            try
            {
                if(dto.Count() > 0)
                {
                    var fields = _mapper.Map<List<Field>>(dto);
                    foreach(var field in fields)
                    {
                        field.CreatedBy = userId;
                        field.CreatedDate = DateTime.UtcNow;
                        _repo.Create(field);
                        await _repo.SaveAsync();
                    }

                    response.Success = true;
                    response.Message = "Fields has been successfully added";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error adding a fields";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddFields: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> DeleteFields(string username)
        {
            var response = new Result();
            try
            {
                var fields = await _repo.GetAsync<Field>(filter: c => c.IsDefault == true);
                if (fields.Count() > 0)
                {
                    foreach (var field in fields)
                    {
                        _repo.Delete(field);
                        await _repo.SaveAsync();
                    }

                    response.Success = true;
                    response.Message = "Fields has been successfully deleted";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error deleting a fields";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteFields: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> AddModules(List<AddModuleDTO> dto, string userId)
        {
            var response = new Result();
            try
            {
                if (dto.Count() > 0)
                {
                    var modules = _mapper.Map<List<Module>>(dto);
                    foreach (var module in modules)
                    {
                        var selectedModule = await _repo.GetAsync<Module>(filter: c => c.Name == module.Name);

                        if (selectedModule.Count() == 0)
                        {
                            module.CreatedBy = userId;
                            module.CreatedDate = DateTime.UtcNow;

                            _repo.Create(module);
                            await _repo.SaveAsync();
                        }
                    }

                    response.Success = true;
                    response.Message = "Modules has been successfully added";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error adding a modules";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddModules: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> AddDocument(AddDocumentDTO dto, List<IFormFile> file, List<IFormFile> datasetfile, string userRole, string userId, string userDept)
        {
            var response = new Result();
            try
            {
                var fileCnt = 0;
                if(file.Count() > 0)
                {
                    foreach (var docField in dto.DocumentField)
                    {
                        if (docField.File)
                        {
                            var g = Guid.NewGuid();
                            var fileName = file[fileCnt].FileName;
                            string fullPath = Path.Combine(_ddpaOptions.Value.AttachmentPath, g + Path.GetExtension(fileName));
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await file[fileCnt].CopyToAsync(stream);
                            }
                            fileCnt += 1;
                            docField.Value = fileName;
                            docField.FilePath = fullPath;
                        }
                    }
                }

                var datasetFileCnt = 0;
                if (datasetfile.Count() > 0)
                {
                    foreach (var docDatasetField in dto.DocumentDatasetField)
                    {
                        if (docDatasetField.File)
                        {
                            var g = Guid.NewGuid();
                            var fileName = file[fileCnt].FileName;
                            string fullPath = Path.Combine(_ddpaOptions.Value.AttachmentPath, g + Path.GetExtension(fileName));
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await datasetfile[datasetFileCnt].CopyToAsync(stream);
                            }
                            datasetFileCnt += 1;
                            docDatasetField.Value = fileName;
                            docDatasetField.FilePath = fullPath;
                        }
                    }
                }

                var document = _mapper.Map<Document>(dto);
                document.CreatedBy = userId;
                document.CreatedDate = DateTime.UtcNow;
                document.DepartmentId = userDept;

                if (userRole == Role.USER.ToString())
                {
                    //if save button is clicked, the state is Draft
                    document.State = (dto.ButtonAction == ButtonAction.Save) ? State.Draft : State.Pending;
                    document.RequestType = RequestType.Add;
                }

                else if (userRole == Role.DEPTHEAD.ToString())
                {
                    document.State = (dto.ButtonAction == ButtonAction.Save) ? State.Draft : State.Pending;
                    document.RequestType = RequestType.Add;
                }

                else if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    document.State = (dto.ButtonAction == ButtonAction.Save) ? State.Draft : State.Approved;
                    document.RequestType = RequestType.Add;
                }

                var x = document.Id.ToString();
                var dataNum = await _queryService.GetAllDocuments();
                document.DataNumber = (dataNum.Count() + 1).ToString();
                _repo.Create(document);
                await _repo.SaveAsync();

                if ((userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)) && dto.ButtonAction == ButtonAction.Save)
                {
                    var wf = await AddToWorkflowInbox(userRole, userId, userDept, document.Id.ToString(), dto.Status);
                }

                else if (userRole != nameof(Role.DPO))
                {
                    var wf = await AddToWorkflowInbox(userRole, userId, userDept, document.Id.ToString(), dto.Status);
                }

                //Add to logs
                LogsDTO dtoLogs = new LogsDTO();
                dtoLogs.DocId = document.Id.ToString(); ;
                dtoLogs.DataNumber = document.DataNumber;
                dtoLogs.UserId = userId;
                dtoLogs.Action = "Add";
                dtoLogs.Description = "Request to add document";
                var log = await AddLogs(dtoLogs);

                response.Id = document.Id.ToString();
                response.Success = true;
                response.Message = "Document has been successfully added";
                response.ErrorCode = ErrorCode.DEFAULT;
            }
            catch (Exception e)
            {
                response.Message = "Error adding a document";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddDocument: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> UpdateDocument(UpdateDocumentDTO dto, List<IFormFile> file, List<IFormFile> datasetfile, int subModuleId, string userId)
        {
            var response = new Result();
            try
            {
                var valResult = await _validationService.IsDataNumberExist(dto.DataNumber, dto.Id);
                if (!valResult.IsValid)
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                var fileCnt = 0;
                foreach (var docField in dto.DocumentField)
                {
                    if (docField.File)
                    {
                        var currentField = await _repo.GetAsync<DocumentField>(
                           filter: (c => c.FieldId == docField.FieldId && c.SubModuleId == docField.SubModuleId));

                        if(currentField.Count() > 0)
                        {
                            if (file.Count() > 0)
                            {
                                if (!String.IsNullOrEmpty(docField.Value))
                                {
                                    var filePath = currentField.FirstOrDefault().FilePath;
                                    if (!String.IsNullOrEmpty(filePath))
                                    {
                                        File.Delete(filePath);
                                    }

                                    var g = Guid.NewGuid();
                                    var fileName = file[fileCnt].FileName;
                                    string fullPath = Path.Combine(_ddpaOptions.Value.AttachmentPath, g + Path.GetExtension(fileName));
                                    using (var stream = new FileStream(fullPath, FileMode.Create))
                                    {
                                        await file[fileCnt].CopyToAsync(stream);
                                    }
                                    fileCnt += 1;
                                    docField.Value = fileName;
                                    docField.FilePath = fullPath;
                                }
                            }
                        }
                    }
                }

                var datasetFileCnt = 0;
                foreach (var docDatasetField in dto.DocumentDatasetField)
                {
                    if (docDatasetField.File)
                    {
                        var currentField = await _repo.GetAsync<DocumentDatasetField>(
                           filter: (c => c.FieldId == docDatasetField.FieldId && c.DatasetId == docDatasetField.DatasetId && docDatasetField.Id == docDatasetField.DocumentId));

                        if (currentField.Count() > 0)
                        {
                            if (datasetfile.Count() > 0)
                            {
                                if (!String.IsNullOrEmpty(docDatasetField.Value))
                                {
                                    var filePath = currentField.FirstOrDefault().FilePath;
                                    if (!String.IsNullOrEmpty(filePath))
                                    {
                                        File.Delete(filePath);
                                    }

                                    var g = Guid.NewGuid();
                                    var fileName = datasetfile[fileCnt].FileName;
                                    string fullPath = Path.Combine(_ddpaOptions.Value.AttachmentPath, g + Path.GetExtension(fileName));
                                    using (var stream = new FileStream(fullPath, FileMode.Create))
                                    {
                                        await datasetfile[datasetFileCnt].CopyToAsync(stream);
                                    }
                                    datasetFileCnt += 1;
                                    docDatasetField.Value = fileName;
                                    docDatasetField.FilePath = fullPath;
                                }
                            }
                        }
                    }
                }

                var documents = await _repo.GetAsync<Document>(
                       filter: (c => c.Id == dto.Id),
                       include: source => source.Include(c => c.DocumentDatasetField).Include(c => c.DocumentField).ThenInclude(c => c.Field).ThenInclude(c => c.FieldItem)
                   );

                var doc = new Document();
                doc = documents.FirstOrDefault();
                var additionalField = _mapper.Map<List<DocumentField>>(dto.DocumentField);
                var additionalDatasetField = _mapper.Map<List<DocumentDatasetField>>(dto.DocumentDatasetField);

                var docFields = doc.DocumentField;
                if(docFields.Count() > 0)
                {
                    foreach (var field in additionalField)
                    {
                        var docField = docFields.Where(x => x.FieldId.Equals(field.FieldId) && x.SubModuleId == (Convert.ToInt32(field.SubModuleId))).Select(x => x);
                        if (docField.Count() == 0)
                            doc.DocumentField.Add(field);
                        else
                        {
                            docField.FirstOrDefault().Value = field.Value;

                            //if document is saved without changing file upload
                            if(field.FilePath == null || field.FilePath == "")
                            {
                                if(field.Value == null || field.Value == "")
                                {
                                    docField.FirstOrDefault().FilePath = field.FilePath;
                                }
                            }

                            //file path will only be not null if a file is selected
                            else
                            {
                                docField.FirstOrDefault().FilePath = field.FilePath;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var field in additionalField)
                    {
                        doc.DocumentField.Add(field);
                    }
                }

                var docDatasetFields = doc.DocumentDatasetField.Where(x => x.DatasetId == dto.DatasetId);
                if (docDatasetFields.Count() > 0)
                {
                    foreach (var tempateField in additionalDatasetField)
                    {
                        var docDatasetField = docDatasetFields.Where(x => x.FieldId.Equals(tempateField.FieldId) && x.DatasetId == (Convert.ToInt32(dto.DatasetId))).Select(x => x);
                        if (docDatasetField.Count() == 0)
                            doc.DocumentDatasetField.Add(tempateField);
                        else
                            docDatasetField.FirstOrDefault().Value = tempateField.Value;
                    }
                }
                else
                {
                    foreach (var tempateField in additionalDatasetField)
                    {
                        doc.DocumentDatasetField.Add(tempateField);
                    }
                }

                if (Convert.ToInt32(doc.Status) < Convert.ToInt32(dto.Status))
                {
                    doc.Status = dto.Status;
                }

                if(dto.Status == Status.Archival)
                {
                    //todo: fieldid is hardcoded base on seeding. Rework
                    var retentionPeriodValue = dto.DocumentField.First(i => i.FieldId == 30).Value;
                    doc.DueDate = doc.CreatedDate.AddMonths(Convert.ToInt32(retentionPeriodValue));
                }

                doc.ModifiedBy = userId;
                doc.ModifiedDate = DateTime.UtcNow;

                _repo.Update(doc);
                await _repo.SaveAsync();

                response.Id = dto.Id.ToString();
                response.Message = "Document has been successfully updated";
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error updating the document";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateDocument: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> AddDatasets(List<AddDatasetDTO> dto, string userId)
        {
            var response = new Result();
            try
            {
                if (dto.Count() > 0)
                {
                    var datasets = _mapper.Map<List<Dataset>>(dto);
                    foreach (var dataset in datasets)
                    {
                        var selectedDataset = await _repo.GetAsync<Dataset>(filter: c => c.Name == dataset.Name);

                        if (selectedDataset.Count() == 0)
                        {
                            dataset.CreatedBy = userId;
                            dataset.CreatedDate = DateTime.UtcNow;

                            _repo.Create(dataset);
                            await _repo.SaveAsync();
                        }
                    }

                    response.Success = true;
                    response.Message = "Datasets has been successfully added";
                    response.ErrorCode = ErrorCode.DEFAULT;
                }
            }
            catch (Exception e)
            {
                response.Message = "Error adding a datasets";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddDatasets: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> BulkUploadXlsOrXlsx(string name, string folder, string fileExt, string currentUser, string rootFolder, string userRole, string userId, string userDept)
        {
            Result result = new Result();
            List<IFormFile> tempFile = new List<IFormFile>();
            List<IFormFile> tempDatasetFile = new List<IFormFile>();
            try
            {
                string fileName = name + fileExt;
                FileInfo file = new FileInfo(Path.Combine(rootFolder, folder, fileName));

                ExcelPackage package = new ExcelPackage(file);
                StringBuilder sb = new StringBuilder();
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                int rowCount = workSheet.Dimension.Rows;
                int ColCount = workSheet.Dimension.Columns;

                //collection's list of field
                var subModuleField = await _queryService.GetAllSubModuleFields();

                //if row is only header
                if (rowCount <= 1)
                {
                    result.Message = "No data found.";
                    result.Success = false;
                    package.Dispose();
                    return result;
                }

                //check if submodule field and excel header count are equals
                if (subModuleField.Count != ColCount - 1)
                {
                    result.Success = false;
                    result.Message = "Error! Invalid excel header count.";
                    package.Dispose();
                    return result;
                }

                //DTO that will be the parameter in adding data
                List<AddDocumentDTO> listOfDocumentDTO = new List<AddDocumentDTO>();

                //loop through rows, should start at row 2 because row 1 is header
                for (int row = 2; row <= rowCount; row++)
                {
                    //fields of the document. Example is Department. This will contain the field ID and the value in excel
                    List<DocumentFieldDTO> fieldDTO = new List<DocumentFieldDTO>();

                    //Document that will be added in list of documents. Fields will initially saved here
                    AddDocumentDTO tempDocumentDTO = new AddDocumentDTO();

                    //default status is Collection, because initial module when adding document is collection
                    tempDocumentDTO.Status = Status.Collection;

                    //default dataset is 0, because we are not yet using dataset in bulk uploading
                    tempDocumentDTO.DatasetId = 0;

                    //TODO: add validation if datanumber is null
                    //hard coded yes, assuming the data number is always in column 1 of each row
                    tempDocumentDTO.DataNumber = workSheet.Cells[row, 1].Value.ToString();

                    //loop through each columns, left to right.
                    for (int col = 1; col <= ColCount; col++)
                    {
                        if (col != 1)
                        {
                            //if field name is not equal to excel header ex: Department and Data Number, pass if column is data number, pass if field is attachment
                            if (subModuleField[col - 2].Field.Name != workSheet.Cells[1, col].Value.ToString())
                            {
                                result.Message = "Error! Invalid excel header name.";
                                package.Dispose();
                                result.Success = false;
                                return result;
                            }
                            //create a field dto, this field will be added in its list.
                            DocumentFieldDTO tempFieldDTO = new DocumentFieldDTO();

                            //save the field id of read field.
                            tempFieldDTO.FieldId = Convert.ToInt32(subModuleField[col - 2].Field.Id);

                            //populate via hard coding.
                            tempFieldDTO.File = false;
                            tempFieldDTO.Id = 0;

                            //save the submodule id from read sub module.
                            tempFieldDTO.SubModuleId = subModuleField[col-2].SubModuleId;

                            //check if the cell with value has value and not null
                            string cellValue = (workSheet.Cells[row, col].Value != null) ? workSheet.Cells[row, col].Value.ToString() : "";

                            //do not check value if column is attachment
                            if (subModuleField[col - 2].Field.Type != FieldType.Attachment)
                            {
                                if (cellValue == null || cellValue == "")
                                {
                                    result.Message = "Error! One or more cell has no data.";
                                    result.Success = false;
                                    package.Dispose();
                                    return result;
                                }
                            }

                            //if column is dropdown
                            if (subModuleField[col - 2].Field.Type == FieldType.ComboField)
                            {
                                //check if the cell value exist in dropdown's item
                                var validationResult = await _validationService.FieldItemExist(subModuleField[col - 2].Field.Id, cellValue);

                                //if the cell value does not exist, return error
                                if (!validationResult.IsValid)
                                {
                                    result.Message = "Error! \"" + cellValue + "\" does not exist in " + subModuleField[col - 2].Field.Name + "'s item.";
                                    result.Success = false;
                                    package.Dispose();
                                    return result;
                                }
                            }

                            //save the value of the field. This is the cell that contains the field's value.
                            tempFieldDTO.Value = cellValue;

                            //save the temp field dto to its list.
                            fieldDTO.Add(tempFieldDTO);
                        }

                    }
                    //add fields with data every row to document.
                    tempDocumentDTO.DocumentField = fieldDTO;

                    //save the document created to its lsit.
                    listOfDocumentDTO.Add(tempDocumentDTO);
                }


                foreach (AddDocumentDTO document in listOfDocumentDTO)
                {
                    result = await AddDocument(document, tempFile, tempDatasetFile, userRole, userId, userDept);
                }
                package.Dispose();
            }
            catch (Exception e)
            {
                result.Message = "Error bulk uploading.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling BulkUploadXlsOrXlsx: {0}", e.Message);
            }

            result.IsRedirect = true;
            result.RedirectUrl = "Dataset/DataList";
            return result;
        }

        public async Task<Result> BulkUploadCsv(string name, string folder, string fileExt, string currentUser, string rootFolder, string userRole, string userId, string userDept, string docDeptId, string docDataSetId)
        {
            Result result = new Result();
            List<IFormFile> tempFile = new List<IFormFile>();
            List<IFormFile> tempDatasetFile = new List<IFormFile>();
            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            try
            {
                string fileName = name + fileExt;

                //read csv file
                var csvReader = new StreamReader(Path.Combine(rootFolder, folder, fileName));

                //csv header
                List<string> csvHeader = new List<string>();

                //DTO that will be the parameter in adding data
                List<AddDocumentDTO> listOfDocumentDTO = new List<AddDocumentDTO>();

                //Check row count if exceeds 101
                //101 because the row limit is 100 and plus the header
                if(File.ReadLines(Path.Combine(rootFolder, folder, fileName)).Count() > 101)
                {
                    result.Success = false;
                    result.Message = "Error! The allowed maximum number of records is 100 only.";
                    csvReader.Dispose();
                    return result;
                }
                while (!csvReader.EndOfStream)
                {
                    //get csv header and save in a string
                    var rowString = csvReader.ReadLine();

                    //first iteration
                    if (csvHeader.Count == 0)
                    {
                        //string csv header to list of column header, coma as delimiter
                        csvHeader = rowString.Split(',').ToList();
                        continue;
                    }

                    List<string> csvRowData = new List<string>();

                    //row data
                    csvRowData = CSVParser.Split(rowString).ToList();
                    for (int i = 0; i < csvRowData.Count; i++)
                    {
                        csvRowData[i] = csvRowData[i].TrimStart(' ', '"');
                        csvRowData[i] = csvRowData[i].TrimEnd('"');
                    }

                     //sub module's field
                    var subModulesField = await _queryService.GetAllSubModuleFields();

                    if (subModulesField.Count != csvHeader.Count)
                    {
                        result.Success = false;
                        result.Message = "Error! Invalid csv header count. Please download and use the latest csv template.";
                        csvReader.Dispose();
                        return result;
                    }
                    //fields of the document. Example is Department. This will contain the field ID and the value in excel
                    List<DocumentFieldDTO> fieldDTO = new List<DocumentFieldDTO>();

                    //Document that will be added in list of documents. Fields will initially saved here
                    AddDocumentDTO tempDocumentDTO = new AddDocumentDTO();

                    //default status is Collection, because initial module when adding document is collection
                    tempDocumentDTO.Status = Status.Collection;

                    //add the data set that the user selected
                    tempDocumentDTO.DatasetId = Convert.ToInt32(docDataSetId);

                    //add the data set that the user selected
                    tempDocumentDTO.DepartmentId = Convert.ToInt32(docDeptId);


                    int nextColumn = 0;
                    for (int i = 1; i <= subModulesField.Count; i++)
                    {
                        nextColumn++;
                    }
                    //loop through each columns, left to right.
                    for (int col = 0; col < csvRowData.Count; col++)
                    {
                        //if field name is not equal to excel header ex: Department and Data Number, pass if column is data number, pass if field is attachment
                        if (subModulesField[col].Field.Name != csvHeader[col])
                        {
                            result.Message = "Error! Invalid csv header name. Please download and use the latest csv template.";
                            csvReader.Dispose();
                            result.Success = false;
                            return result;
                        }
                        //create a field dto, this field will be added in its list.
                        DocumentFieldDTO tempFieldDTO = new DocumentFieldDTO();

                        //save the field id of read field.
                        tempFieldDTO.FieldId = Convert.ToInt32(subModulesField[col].Field.Id);

                        //populate via hard coding.
                        tempFieldDTO.File = false;
                        tempFieldDTO.Id = 0;

                        //save the submodule id from read sub module.
                        tempFieldDTO.SubModuleId = subModulesField[col].SubModuleId;

                        string cellValue = csvRowData[col];

                        //do not check value if column is attachment
                        if (subModulesField[col].Field.Type != FieldType.Attachment)
                        {
                            
                        }

                        //if column is dropdown
                        if (subModulesField[col].Field.Type == FieldType.ComboField && cellValue != "")
                        {
                            //split multiple dropdown into string
                            //ex: "locker,database" to string[0] = locker, string[1] = database
                            List<string> cellValueItems = cellValue.Split(',').ToList();

                            foreach(string item in cellValueItems)
                            {
                                //check if the cell value exist in dropdown's item
                                var validationResult = await _validationService.FieldItemExist(subModulesField[col].Field.Id, item);

                                //if the cell value does not exist, return error
                                if (!validationResult.IsValid)
                                {
                                    result.Message = "Error! \"" + item + "\" does not exist in " + subModulesField[col].Field.Name + "'s item.";
                                    result.Success = false;
                                    csvReader.Dispose();
                                    return result;
                                }
                            }
                            
                        }

                        if (subModulesField[col].Field.Type == FieldType.Checkbox && cellValue != "")
                        {
                            if(cellValue.ToLower() != "true" && cellValue.ToLower() != "false")
                            {
                                result.Message = "Error! \"" + subModulesField[col].Field.Name + "\" has invalid data. Allowed data are \"true\" and \"false\".";
                                result.Success = false;
                                csvReader.Dispose();
                                return result;
                            }
                        }

                        //save the value of the field. This is the cell that contains the field's value.
                        tempFieldDTO.Value = cellValue;

                        //save the temp field dto to its list.
                        fieldDTO.Add(tempFieldDTO);
                    }

                    //add fields with data every row to document.
                    tempDocumentDTO.DocumentField = fieldDTO;

                    //save the document created to its lsit.
                    listOfDocumentDTO.Add(tempDocumentDTO);
                }

                //if csv has only header
                if (listOfDocumentDTO.Count <= 0)
                {
                    result.Message = "No data found.";
                    csvReader.Dispose();
                    result.Success = false;
                    return result;
                }

                foreach (AddDocumentDTO document in listOfDocumentDTO)
                {
                    result = await AddDocument(document, tempFile, tempDatasetFile, userRole, userId, userDept);

                    //if there is an error in adding document
                    if (!result.Success)
                    {
                        break;
                    }
                }
                csvReader.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError("Error calling UpdateDocument: {0}", e.Message);
                throw;
            }

            result.IsRedirect = true;
            result.RedirectUrl = "Dataset/DataList";
            return result;
        }

        public async Task<Result> EditDocument(UpdateDocumentDTO dto, int subModuleId, string userRole, string userId, string userdept)
        {
            var response = new Result();
            try
            {
                var valResult = await _validationService.IsDataNumberExist(dto.DataNumber, dto.Id);
                if (!valResult.IsValid)
                {
                    response.Message = valResult.Message;
                    response.ErrorCode = ErrorCode.INVALID_INPUT;
                    return response;
                }

                var documents = await _repo.GetAsync<Document>(
                       filter: (c => c.Id == dto.Id),
                       include: source => source.Include(c => c.DocumentDatasetField).Include(c => c.DocumentField).ThenInclude(c => c.Field).ThenInclude(c => c.FieldItem)
                   );

                var doc = new Document();
                State docState = documents.First().State;
                doc = documents.FirstOrDefault();
                var additionalField = _mapper.Map<List<DocumentField>>(dto.DocumentField);
                var additionalDatasetField = _mapper.Map<List<DocumentDatasetField>>(dto.DocumentDatasetField);

                var docFields = doc.DocumentField;

                if (docFields.Count() > 0)
                {
                    foreach (var field in additionalField)
                    {
                        var docField = docFields.Where(x => x.FieldId.Equals(field.FieldId) && x.SubModuleId == (Convert.ToInt32(field.SubModuleId))).Select(x => x);

                        if (docField.Count() == 0)
                        {
                            field.IsEdited = true;
                            field.NewValue = field.Value;
                            field.Value = null;
                            doc.DocumentField.Add(field);

                        }
                        else
                        {
                            if (docState == State.Draft || docState == State.Rework)
                            {
                                if(doc.RequestType == RequestType.Add)
                                {
                                    docField.FirstOrDefault().Value = field.Value;
                                }
                                else if(doc.RequestType == RequestType.Edit)
                                {
                                    docField.FirstOrDefault().NewValue = field.Value;
                                    docField.FirstOrDefault().Value = docField.FirstOrDefault().Value;
                                    docField.FirstOrDefault().IsEdited = true;
                                }
                            }
                            else if (doc.State == State.Approved && (userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR)))
                            {
                                if (docField.FirstOrDefault().NewValue != field.Value)
                                {
                                    docField.FirstOrDefault().Value = field.Value;
                                }
                            }
                            else
                            {
                                if (docField.FirstOrDefault().Value != field.Value)
                                {
                                    docField.FirstOrDefault().NewValue = field.Value;
                                    docField.FirstOrDefault().Value = docField.FirstOrDefault().Value;
                                    docField.FirstOrDefault().IsEdited = true;

                                }
                            }

                        } 
                    }
                }
                else
                {
                    foreach (var field in additionalField)
                    {
                        field.IsEdited = true;
                        field.NewValue = field.Value;
                        field.Value = null;
                        doc.DocumentField.Add(field);
                    }
                }

                if(userRole == nameof(Role.USER) || userRole == nameof(Role.DEPTHEAD))
                {
                    //If data set is draft and the user submits it, update its workflow
                    if (doc.State == State.Rework && (dto.ButtonAction == ButtonAction.Submit || dto.ButtonAction == ButtonAction.Save))
                    {
                        var wfi = await _repo.GetFirstAsync<WorkflowInbox>(
                        filter: (c => c.DocumentId == doc.Id.ToString()));
                        //if the Role is USER
                        if (userRole == nameof(Role.USER))
                        {
                            wfi.ApproverRole = nameof(Role.DEPTHEAD);
                        }//if the Role is DEPTHEAD
                        else if (userRole == nameof(Role.DEPTHEAD))
                        {
                            wfi.ApproverRole = nameof(Role.DPO);
                        }
                        wfi.ModifiedBy = userId;
                        wfi.ModifiedDate = DateTime.UtcNow;

                        _repo.Update(wfi);
                        await _repo.SaveAsync();
                    }
                    else
                    {
                        //if document is from draft, it already has a workflow
                        if (docState != State.Draft && userRole != nameof(Role.DPO))
                        {
                            if((await _repo.GetAsync<WorkflowInbox>(
                                filter: f => f.DocumentId == doc.Id.ToString())).Count() == 0)
                            {
                                //Add to WFInbox if request is Edit
                                var wf = await AddToWorkflowInbox(userRole, userId, userdept, doc.Id.ToString(), dto.Status);
                            }
                        }
                    }

                    //update Document's status
                    if(dto.ButtonAction == ButtonAction.Submit)
                    {
                        if (docState == State.Draft)
                        {
                            //RequestType is add. Because the user hasn't submitted it yet
                            doc.RequestType = RequestType.Add;
                            doc.State = State.Pending;
                        }
                        else if (docState == State.Approved)
                        {
                            //RequestType is add. Because the user hasn't submitted it yet
                            doc.RequestType = RequestType.Edit;
                            doc.State = State.Pending;
                        }
                        else if (docState == State.Rework)
                        {
                            //RequestType is add. Because the user hasn't submitted it yet
                            doc.State = State.Pending;
                        }
                    }
                    else if (dto.ButtonAction == ButtonAction.Save)
                    {
                        doc.RequestType = RequestType.Add;
                        doc.State = State.Draft;
                    }
                    
                }
                else if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    //if DPO Submit request
                    if (dto.ButtonAction == ButtonAction.Submit) {
                        //remove to WorkflwInbox
                        var wfi = await _repo.GetFirstAsync<WorkflowInbox>(
                        filter: (c => c.DocumentId == doc.Id.ToString()));
                        if(wfi != null)
                        {
                        _repo.Delete(wfi);
                        _repo.Save();
                        }

                        doc.State = State.Approved; 
                    }
                }
                
                doc.ModifiedBy = userId;
                doc.ModifiedDate = DateTime.UtcNow;

                _repo.Update(doc);
                await _repo.SaveAsync();

                //Add to logs
                LogsDTO dtoLogs = new LogsDTO();
                dtoLogs.DocId = doc.Id.ToString(); 
                dtoLogs.DataNumber = doc.DataNumber;
                dtoLogs.UserId = userId;
                dtoLogs.Action = "Edit";
                dtoLogs.Description = "Request to edit document";
                var log = await AddLogs(dtoLogs);

                response.Id = dto.Id.ToString();
                response.Message = "Document has been successfully updated";
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error updating the document";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling UpdateDocument: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> AddToWorkflowInbox(string userRole, string userid, string userdept, string docId, Status status)
        {
            var response = new Result();
            try
            {
                string approverrole = "";
                if (userRole == Role.USER.ToString())
                {                    
                    approverrole = Role.DEPTHEAD.ToString();
                }

                else if (userRole == Role.DEPTHEAD.ToString())
                {
                    approverrole = Role.DPO.ToString();
                }
                //save the document in workflow
                WorkflowInbox wfi = new WorkflowInbox
                {
                    ApproverRole = approverrole,
                    CreatedBy = userid,
                    DepartmentId = userdept,
                    DocumentId = docId,
                    Status = status
                };

                _repo.Create(wfi);
                await _repo.SaveAsync();

            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error add the document to inbox";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddToWorkflowInbox: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> DeleteDataSet(string userRole, string userId, string userDept, string docId)
        {
            var result = new Result();
            try
            {
                var document = _repo.GetById<Document>(Convert.ToInt32(docId));
                
                if(userRole == nameof(Role.DPO) || userRole == nameof(Role.ADMINISTRATOR))
                {
                    if (document == null)
                    {
                        result.Message = "Document does not exist.";
                        result.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                        return result;
                    }

                    //Update the document, deleting process
                    document.State = State.Approved;
                    document.RequestType = RequestType.Delete;

                    _repo.Update(document);
                    await _repo.SaveAsync();
                    
                    //Add to logs
                    LogsDTO dtoLogs = new LogsDTO();
                    dtoLogs.DocId = document.Id.ToString(); ;
                    dtoLogs.DataNumber = document.DataNumber;
                    dtoLogs.UserId = userId;
                    dtoLogs.Action = "Delete";
                    dtoLogs.Description = "Document deleted by " + userRole;
                    var log = await AddLogs(dtoLogs);

                    result.Message = "Document has been successfully deleted.";
                    result.Success = true;
                }

                else if(userRole == nameof(Role.DEPTHEAD) || userRole == nameof(Role.USER))
                {
                    if (document == null)
                    {
                        result.Message = "Document does not exist.";
                        result.ErrorCode = ErrorCode.DATA_NOT_FOUND;
                        return result;
                    }

                    //Update the document, deleting process
                    document.State = State.Pending;
                    document.RequestType = RequestType.Delete;

                    _repo.Update(document);
                    await _repo.SaveAsync();

                    if ((await _repo.GetAsync<WorkflowInbox>(
                                filter: f => f.DocumentId == document.Id.ToString())).Count() == 0)
                    {
                        //add the document in the inbox for the approvers
                        await AddToWorkflowInbox(userRole, userId, userDept, document.Id.ToString(), document.Status);
                    }
                    //Add to logs
                    LogsDTO dtoLogs = new LogsDTO();
                    dtoLogs.DocId = document.Id.ToString(); ;
                    dtoLogs.DataNumber = document.DataNumber;
                    dtoLogs.UserId = userId;
                    dtoLogs.Action = "Delete";
                    dtoLogs.Description = "Request to delete document";
                    var log = await AddLogs(dtoLogs);

                    result.Message = "Document's deletion has been successfully sent for approval.";
                    result.Success = true;
                }

                
            }
            catch (Exception e)
            {
                result.Message = "Error deleting document.";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling DeleteDataSet: {0}", e.Message);
            }

            return result;
        }

        public async Task<Result> AddLogs(LogsDTO dto)
        {
            var response = new Result();
            try
            {
                dto.ActionDate = DateTime.UtcNow;
                var logs = _mapper.Map<Logs>(dto);

                _repo.Create(logs);
                await _repo.SaveAsync();

                response.Message = "Logs has been successfully Created";
                response.Success = true;
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Error Adding Logs";
                response.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling AddToLogs: {0}", e.Message);
            }

            return response;
        }

        public async Task<Result> CreateUserGuideResource(AddResourceDTO dto, string userId)
        {
            var result = new Result();
            try
            {
                var resource = new Resource
                {
                    NameOfDocument = dto.NameOfDocument,
                    TypeOfDocument = dto.TypeOfDocument,
                    FilePath = dto.FilePath,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId
                };
                _repo.Create(resource);
                await _repo.SaveAsync();

                result.Id = resource.Id.ToString();
                result.Message = "Resource has been successfully Added.";
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Message = "Error adding resource";
                result.ErrorCode = ErrorCode.EXCEPTION;
                _logger.LogError("Error calling CreateResource: {0}", e.Message);
            }

            return result;
        }
    }
}