#!/usr/bin/env python
# coding: utf-8

# ---
# 
# _You are currently looking at **version 1.2** of this notebook. To download notebooks and datafiles, as well as get help on Jupyter notebooks in the Coursera platform, visit the [Jupyter Notebook FAQ](https://www.coursera.org/learn/python-data-analysis/resources/0dhYG) course resource._
# 
# ---

# # Assignment 2 - Pandas Introduction
# All questions are weighted the same in this assignment.
# ## Part 1
# The following code loads the olympics dataset (olympics.csv), which was derrived from the Wikipedia entry on [All Time Olympic Games Medals](https://en.wikipedia.org/wiki/All-time_Olympic_Games_medal_table), and does some basic data cleaning. 
# 
# The columns are organized as # of Summer games, Summer medals, # of Winter games, Winter medals, total # number of games, total # of medals. Use this dataset to answer the questions below.

# In[3]:


import pandas as pd

df = pd.read_csv('olympics.csv', index_col=0, skiprows=1)

for col in df.columns:
    if col[:2]=='01':
        df.rename(columns={col:'Gold'+col[4:]}, inplace=True)
    if col[:2]=='02':
        df.rename(columns={col:'Silver'+col[4:]}, inplace=True)
    if col[:2]=='03':
        df.rename(columns={col:'Bronze'+col[4:]}, inplace=True)
    if col[:1]=='№':
        df.rename(columns={col:'#'+col[1:]}, inplace=True)

names_ids = df.index.str.split('\s\(') # split the index by '('

df.index = names_ids.str[0] # the [0] element is the country name (new index) 
df['ID'] = names_ids.str[1].str[:3] # the [1] element is the abbreviation or ID (take first 3 characters from that)

df = df.drop('Totals')
df.head()


# ### Question 0 (Example)
# 
# What is the first country in df?
# 
# *This function should return a Serie

# In[2]:


# You should write your whole answer within the function provided. The autograder will call
# this function and compare the return value against the correct solution value
def answer_zero():
    # This function returns the row for Afghanistan, which is a Series object. The assignment
    # question description will tell you the general format the autograder is expecting
    return df.iloc[0]

# You can examine what your function returns by calling it in the cell. If you have questions
# about the assignment formats, check out the discussion forums for any FAQs
answer_zero() 


# ### Question 1
# Which country has won the most gold medals in summer games?
# 
# *This function should return a single string value.*

# In[3]:


def answer_one():
    return df[df['Gold'] == df['Gold'].max()].iloc[0].name


answer_one()


# ### Question 2
# Which country had the biggest difference between their summer and winter gold medal counts?
# 
# *This function should return a single string value.*

# In[4]:


def answer_two():
    df['diff'] = df['Gold'] - df['Gold.1']
    return df[df['diff'] == df['diff'].max()].iloc[0].name

answer_two()


# ### Question 3
# Which country has the biggest difference between their summer gold medal counts and winter gold medal counts relative to their total gold medal count? 
# 
# $$\frac{Summer~Gold - Winter~Gold}{Total~Gold}$$
# 
# Only include countries that have won at least 1 gold in both summer and winter.
# 
# *This function should return a single string value.*

# In[5]:


def answer_three():
    #goldInBoth = df.copy()
    goldInBoth = goldInBoth[(goldInBoth['Gold'] > 0)  & (goldInBoth['Gold.1'] > 0)]
    goldInBoth['diff'] = (goldInBoth['Gold'] - goldInBoth['Gold.1'])/goldInBoth['Gold.2']
    print(goldInBoth['diff'].max())
    return goldInBoth[goldInBoth['diff'] == goldInBoth['diff'].max()].iloc[0].name
answer_three()

# ### Question 4
# Write a function that creates a Series called "Points" which is a weighted value where each gold medal (`Gold.2`) counts for 3 points, silver medals (`Silver.2`) for 2 points, and bronze medals (`Bronze.2`) for 1 point. The function should return only the column (a Series object) which you created, with the country names as indices.
# 
# *This function should return a Series named `Points` of length 146*

# In[6]:


def answer_four():
    df['Points'] = df['Gold.2']*3 + df['Silver.2']*2 + df['Bronze.2']*1
   
    return  df['Points']

answer_four()


# ## Part 2
# For the next set of questions, we will be using census data from the [United States Census Bureau](http://www.census.gov). Counties are political and geographic subdivisions of states in the United States. This dataset contains population data for counties and states in the US from 2010 to 2015. [See this document](https://www2.census.gov/programs-surveys/popest/technical-documentation/file-layouts/2010-2015/co-est2015-alldata.pdf) for a description of the variable names.
# 
# The census dataset (census.csv) should be loaded as census_df. Answer questions using this as appropriate.
# 
# ### Question 5
# Which state has the most counties in it? (hint: consider the sumlevel key carefully! You'll need this for future questions too...)
# 
# *This function should return a single string value.*

# In[4]:


census_df = pd.read_csv('census.csv')
                       
census_df.head()


# In[5]:


def answer_five():
    df = pd.DataFrame(columns=['STNAME','NumCtys'])
    #NumCtys_df = pd.DataFrame(census_df[census_df['SUMLEV'] == 50].groupby(level = 0).size().reset_index(name='COUNT'))
    #MaxNumCtys = NumCtys_df['COUNT'].max()
    NumCtys_df = pd.DataFrame(census_df[census_df['SUMLEV'] == 50])
    i = 0
    for state in states:
        #if(state == 'Alabama' or state == 'California' or state == 'Texas'):
        #if(state == 'Alabama'):
            df.loc[i] = [state,NumCtys_df[NumCtys_df['STNAME'] == state]['CTYNAME'].count()]
            
           
            i = i + 1
            #print(NumCtys_df[NumCtys_df['STNAME'] == state]['CTYNAME'].count())

    #print(df)  
    print(df.sort_values('NumCtys',ascending=False).head(1)['STNAME'].iloc[0])
    #print(NumCtys_df)
    #for i, row in NumCtys_df.iterrows():
     #   if(row['COUNT'] == MaxNumCtys ):
       #     MaxCtysState = row['STNAME']
        
    #return MaxCtysState
    return #df.sort_values('NumCtys',ascending=False).head(1)['STNAME'].keys
census_df = pd.read_csv('census.csv')
columns_to_keep = ['SUMLEV','STNAME','CTYNAME']
census_df = census_df[columns_to_keep]
states = census_df['STNAME'].unique()
#census_df = census_df.set_index(['STNAME','CTYNAME'])

answer_five()
#census_df = census_df.reset_index()


# ### Question 6
# **Only looking at the three most populous counties for each state**, what are the three most populous states (in order of highest population to lowest population)? Use `CENSUS2010POP`.
# 
# *This function should return a list of string values.*

# In[127]:


def answer_six():
    df = pd.DataFrame(columns=['STNAME','Top3Pop'])
    #print(df)
    #census_df['MAXPOP'] = pd.DataFrame(census_df[census_df['SUMLEV'] == 50].groupby(level = 0))
    #[census_df['SUMLEV'] == 50].sort_values('CENSUS2010POP',ascending=False).head(3)
    i = 0
    for state in states:
        #if(state == 'Alabama' or state == 'California' or state == 'Texas'):
        #if(state == 'Alabama'):
            this_state = census_df[(census_df['SUMLEV'] == 50) & (census_df['STNAME'] == state)].sort_values('CENSUS2010POP',ascending=False).head(3)
            #print(this_state)
            #this_state_Top3 = this_state.groupby(['STNAME'])['CENSUS2010POP'].sum()
            this_state_Top3 = 0
            #print(this_state_Top3)
            for i,row in this_state.iterrows():      
                this_state_Top3 = this_state_Top3 + row['CENSUS2010POP']
            
            #print(this_state_Top3)
               
            df.loc[i] = [state,this_state_Top3]
            ##this_state = this_state.set_index(['STNAME'])
            #print(this_state)
            #this_state_Top3
            #print(this_state_Top3[0])
            ##this_state['Top3Pop'].loc[state] = this_state_Top3[0]
            #print(this_state)
            ##census_df['Top3Pop'].loc[state] = this_state_Top3[0]
            ##df.loc[i] = [state,this_state_Top3[0]]
            i = i + 1
            #print(census_df['Top3Pop'].loc[state])
            #print(census_df[(census_df['SUMLEV'] == 50) & (census_df['STNAME'] == state)])
        #.sort_values('CENSUS2010POP',ascending=False).head(3))
    #df=df.set_index(['STNAME'])
    print(df.sort_values('Top3Pop',ascending=False).head(3)['STNAME'].tolist())
    return #df['Top3Pop']

census_df = pd.read_csv('census.csv')
columns_to_keep = ['SUMLEV','STNAME','CENSUS2010POP']
census_df = census_df[columns_to_keep]
states = census_df['STNAME'].unique()
census_df['Top3Pop'] = 0
#census_df = census_df.set_index(['STNAME','CTYNAME'])

answer_six()


# ### Question 7
# Which county has had the largest absolute change in population within the period 2010-2015? (Hint: population values are stored in columns POPESTIMATE2010 through POPESTIMATE2015, you need to consider all six columns.)
# 
# e.g. If County Population in the 5 year period is 100, 120, 80, 105, 100, 130, then its largest change in the period would be |130-80| = 50.
# 
# *This function should return a single string value.*

# In[125]:


def answer_seven():
    census_df['largest_Abs_Change'] = census_df[census_df['SUMLEV'] == 50][['POPESTIMATE2010','POPESTIMATE2011','POPESTIMATE2012','POPESTIMATE2013','POPESTIMATE2014','POPESTIMATE2015']].max(axis=1) - census_df[['POPESTIMATE2010','POPESTIMATE2011','POPESTIMATE2012','POPESTIMATE2013','POPESTIMATE2014','POPESTIMATE2015']].min(axis=1)
    largest_Abs_Change = census_df['largest_Abs_Change'].max()
    for i, row in census_df.iterrows():
        if(row['largest_Abs_Change'] == largest_Abs_Change ):
            MaxCty = row['CTYNAME']
    return MaxCty
census_df = pd.read_csv('census.csv')
answer_seven()


# ### Question 8
# In this datafile, the United States is broken up into four regions using the "REGION" column. 
# 
# Create a query that finds the counties that belong to regions 1 or 2, whose name starts with 'Washington', and whose POPESTIMATE2015 was greater than their POPESTIMATE 2014.
# 
# *This function should return a 5x2 DataFrame with the columns = ['STNAME', 'CTYNAME'] and the same index ID as the census_df (sorted ascending by index).*

# In[126]:


def answer_eight():
    return census_df[((census_df['REGION'] == 1) | (census_df['REGION'] == 2)) & (census_df['CTYNAME'].str.startswith('Washington')) & (census_df['POPESTIMATE2015'] > census_df['POPESTIMATE2014'])][['STNAME','CTYNAME']] 
                     
                     
answer_eight()                     
                     
#census_df[census_df['CTYNAME'].str.startswith('Washington')]


# In[ ]:




