getwd()
setwd(".\\")
getwd()
library(readxl)

# The readxl package is already loaded

# Import the first Excel sheet of urbanpop_nonames.xlsx (R gives names): pop_a
pop_a = read_excel("urbanpop_nonames.xlsx",sheet = 1,col_names = FALSE)

# Import the first Excel sheet of urbanpop_nonames.xlsx (specify col_names): pop_b
cols <- c("country", paste0("year_", 1960:1966))

pop_b = read_excel("urbanpop_nonames.xlsx",sheet = 1,col_names = cols)
# Print the summary of pop_a

summary(pop_a)
# Print the summary of pop_b
summary(pop_b)
head(pop_a)
head(pop_b)


urbanpop_sel = read_excel("urbanpop.xlsx",sheet = 2, skip = 21, col_names = FALSE)
urbanpop_sel
# Print out the first observation from urbanpop_sel
urbanpop_sel[1,]
install.packages("gdata")
# Load the gdata package
library(gdata)

# Import the second sheet of urbanpop.xls: urban_pop
# Convert to csv using perl, then reads is as csv like read.csv()
urban_pop <- read.xls("urbanpop.xls",sheet = "1967-1974" , perl = "C:/Strawberry/perl/bin/perl.exe")

# Print the first 11 observations using head()
head(urban_pop,n=11)

# Column names for urban_pop
columns <- c("country", paste0("year_", 1967:1974))
# Finish the read.xls() call that reads data from the second sheet of urbanpop.xls: 
# skip the first 50 rows of the sheet. Make sure to set header appropriately and that the country 
# names are not imported as factors.
urban_pop <- read.xls("urbanpop.xls",sheet = "1967-1974" , 
                      perl = "C:/Strawberry/perl/bin/perl.exe", 
                      skip = 50, header = FALSE, stringsAsFactors = FALSE,
                      col.names = columns)


# Print the first 11 observations using head()
head(urban_pop,n=10)

# Add code to import data from all three sheets in urbanpop.xls
path <- "urbanpop.xls"
urban_sheet1 <- read.xls(path, sheet = 1, perl = "C:/Strawberry/perl/bin/perl.exe" , stringsAsFactors = FALSE)
urban_sheet2 <- read.xls(path, sheet = 2, perl = "C:/Strawberry/perl/bin/perl.exe" , stringsAsFactors = FALSE)
urban_sheet3 <- read.xls(path, sheet = 3, perl = "C:/Strawberry/perl/bin/perl.exe" , stringsAsFactors = FALSE)


# Extend the cbind() call to include urban_sheet3: urban
urban <- cbind(urban_sheet1, urban_sheet2[-1],urban_sheet3[-1])

# Remove all rows with NAs from urban: urban_clean
urban_clean <- na.omit(urban)

# Print out a summary of urban_clean
summary(urban_clean)
head(urban_clean, n=10)

#### XLConnect

library("XLConnect")

my_book <- loadWorkbook("urbanpop.xlsx")

class(my_book)

# Build connection to urbanpop.xlsx
my_book <- loadWorkbook("urbanpop.xlsx")

# List the sheets in my_book
getSheets(my_book)

# Import the second sheet in my_book
readWorksheet(my_book,sheet=2)


my_book <- loadWorkbook("urbanpop.xlsx")
sheets <- getSheets(my_book)
all <- lapply(sheets, readWorksheet, object = my_book)
str(all)


# Build connection to urbanpop.xlsx
my_book <- loadWorkbook("urbanpop.xlsx")

# Import columns 3, 4, and 5 from second sheet in my_book: urbanpop_sel
urbanpop_sel <- readWorksheet(my_book, sheet = 2, startCol = 3,endCol = 5)

# Import first column from second sheet in my_book: countries

countries <- readWorksheet(my_book,sheet = 2, startCol = 1,endCol = 1 )
# cbind() urbanpop_sel and countries together: selection
selection <- cbind(countries,urbanpop_sel)


######### Adding/Removing worksheet

# XLConnect is already available

# Build connection to urbanpop.xlsx
my_book <- loadWorkbook("urbanpop.xlsx")

# Add a worksheet to my_book, named "data_summary"

createSheet(my_book,"data_summary")
# Use getSheets() on my_book
getSheets(my_book)


# Write to worksheet
# Build connection to urbanpop.xlsx
my_book <- loadWorkbook("urbanpop.xlsx")

# Add a worksheet to my_book, named "data_summary"
createSheet(my_book, "data_summary")

# Create data frame: summ
sheets <- getSheets(my_book)[1:3]
dims <- sapply(sheets, function(x) dim(readWorksheet(my_book, sheet = x)), USE.NAMES = FALSE)
summ <- data.frame(sheets = sheets,
                   nrows = dims[1, ],
                   ncols = dims[2, ])

# Add data in summ to "data_summary" sheet

writeWorksheet(my_book,summ,sheet ="data_summary")
# Save workbook as summary.xlsx
saveWorkbook(my_book,file = "summary.xlsx")


### Renaming sheet
# Rename "data_summary" sheet to "summary"
renameSheet(my_book,"data_summary","summary")

# Print out sheets of my_book
getSheets(my_book)

# Save workbook to "renamed.xlsx"
saveWorkbook(my_book,file = "renamed.xlsx")


## removing sheets

# Build connection to renamed.xlsx: my_book

my_book = loadWorkbook("renamed.xlsx")
# Remove the fourth sheet

removeSheet(my_book,sheet = 4)
# Save workbook to "clean.xlsx"
saveWorkbook(my_book,file = "clean.xlsx")
