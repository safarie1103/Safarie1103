# Assignment: ASSIGNMENT 2.1-Test Scores
#
# Name: Safari Edris

# Date: 9 December 2019

# 1. What are the observational units in this study?
# Import scores data
getwd()
setwd("Week2")
getwd()
scores <- read.csv("scores.csv")
# display the structure of data frame-observationl units are: Count,Score,Section
str(scores)  
  # 2. Identify the variables mentioned in the narrative paragraph and determine which are categorical and quantitative?
  # The variable 'Section' is factor and thus categorical. It has two quantitative variables- 'Regular', and 'Sports'
  
  # 3. Create one variable to hold a subset of your data set that contains only the Regular Section and one variable for the 
  # Sportsh Section.
  regulars <- subset(scores,scores$Section == 'Regular')
  head(regulars)
  sports <- subset(scores,scores$Section == 'Sports')
  head(sports)
  # 4. Use the Plot function to plot each Sections scores and the number of students achieving that score. 
  # Use additional Plot Arguments to label the graph and give each axis an appropriate label. 

  
  plot(regulars$Score,regulars$Count,main = "Regular Students", xlab = "Score",ylab = "Count")

  plot(sports$Score,sports$Count,main = "Sports Students", xlab = "Score",ylab = "Count")

  # This plot shows a linear relationship between scores of sports and regular students
  #
  plot(sports$Score,regulars$Score,xlab = "Score by Sports Students",ylab = "Score by regular Students")
  
  # Once you have produced your Plots answer the following questions:
  
  # a. Comparing and contrasting the point distributions between the two section, looking at both tendency and consistency: 
  # Can you say that one section tended to score more points than the other? Justify and explain your answer.
  total_number_of_students <- sum(scores$Count)
  total_number_of_students
  
  total_regs <- sum(regulars$Count)
  total_regs
  reg_above_mean <- sum(subset(regulars, regulars$Score > mean(regulars$Score))$Count)
  reg_above_mean
  reg_below_mean <- sum((subset(regulars, regulars$Score < mean(regulars$Score)))$Count)
  reg_below_mean

  total_sps <- sum(sports$Count)
  total_sps
  sp_above_mean <- sum(subset(sports, sports$Score > mean(sports$Score))$Count)
  sp_above_mean
  sp_below_mean <- sum(subset(sports, sports$Score < mean(sports$Score))$Count)
  sp_below_mean
  
  if (reg_above_mean > sp_above_mean){
    print('more regular students scores above mean than sports students')
  }else {
    print('more sports students scores above mean than regular students')
    
  }
  # b. Did every student in one section score more points than every student in the other section? 
  # The sum of scores shows regular students scored higher than sports student
  
  # Test 1
  if (sum(regulars$Score) > sum(sports$Score)){
    print('Regular students had higher overall score than sports students.')
  } else {
    print('Sports students had higher overall score than  Regular students.')
  }
  # Test 2  
  if (mean(regulars$Score) > mean(sports$Score)){
    print('Regular students had higher mean score than sports students.')
  } else {
    print('Sports students had higher mean score than  Regular students.')
  }
  # Test 3
  if (mean(regulars$Score) > mean(scores$Score)){
    print('Regular students had higher mean score than mean score of all students.')
  }
  # Test 4
  if (mean(sports$Score) > mean(scores$Score)){
    print('Sports students had higher mean score than mean score of all students.')
  }
  
  # The four tests above show that regular students made higher grades in greater number than sports students, but it does not show
  # that every regular student scored more points than sports students.
  # If not, explain what a statistical tendency means in this context.
  
  sd_regs <- sd(regulars$Score)

  sd_sps <- sd(sports$Score)
  
  # Test 5
  if (sd_regs > sd_sps){
    print('Regular students had higher standard deviation than sports student. Therefore they made less score. ')
  }
  if (sd_sps > sd_regs){
    print('Sport students had higher standard deviation than regular student. Therefore they made less score. ')
  }
  # Test 4 further confirms that the regular studentss made better scores.
  
  # c. What could be one additional variable that was not mentioned in the narrative that could be influencing the 
  # point distributions between the two sections?
  # We have scores and number of students that attained that score. would perhaps be the subject matter. 
  # It could be that sime students made better score in greater number in one subject matter but not in others.
  
  
  