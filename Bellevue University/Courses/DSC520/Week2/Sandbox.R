pools <- read.csv("file")
str(pools)

pools <- read.csv("file", stringsAsFactors = FALSE)
str(pools)

pools <- read.csv("file", stringsAsFactors = TRUE)
str(pools)


# Read any tabular file as a data frame
read.table("states2.txt", header = TRUE, sep = "/", stringsAsFactors = FALSE)



# Import hotdogs.txt: hotdogs

hotdogs <- read.delim("hotdogs.txt", header = FALSE)
# Summarize hotdogs
summary(hotdogs)

path <- file.path("data" ,"hotdogs.txt")
hotdogs = read.table(path,sep="\t",col.names = c{"type","Calories","Soduim"})


# Finish the read.delim() call
hotdogs <- read.delim("hotdogs.txt", header = FALSE, col.names = c("type", "calories", "sodium"))

# Select the hot dog with the least calories: lily
lily <- hotdogs[which.min(hotdogs$calories), ]

# Select the observation with the most sodium: tom

tom <- hotdogs[which.max(hotdogs$sodium),]
# Print lily and tom
lily
tom
