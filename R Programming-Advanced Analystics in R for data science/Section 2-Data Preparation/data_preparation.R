getwd()
setwd(".\\")
getwd()
fin <- read.csv("Future-500-The-Dataset.csv", stringsAsFactors = FALSE)
fin
head(fin)
tail(fin,10)
str(fin)
summary(fin)
# changing from non-factor to factor

fin$ID <- factor(fin$ID)
summary(fin)
str(fin)
fin$Inception <- factor((fin$Inception))
summary(fin)
str(fin)


# factor Variable Trap(FVT)
# Converting into Numerics for characters:
a <- c("12","13","14","12","12")
a
typeof(a)
b<- as.numeric(a)
b
typeof(b)
# Converting into Numerics for factors:

z <- factor(c("12","13","14","12","12"))
z
typeof(z)
y <- as.numeric(z)
y
typeof(y)

# --- Correct way
x <- as.numeric(as.character(z))
x
typeof(x)
