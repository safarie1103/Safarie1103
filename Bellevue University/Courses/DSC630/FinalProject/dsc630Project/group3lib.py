# Group 3 Team Library
# group3lib.py
# DSC630 Fall 2020 - Term Project

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

# Set the random seed
np.random.seed(3) 

def read_file():
    
    marketingDf = pd.read_csv("data/datasets_129450_309761_DirectMarketing.csv")
    return marketingDf

def create_scatterplots(data):
    
    plt.scatter(data.Salary,data.AmountSpent)
    plt.xlabel('Salary')
    plt.ylabel('Amount Spent');
    plt.title('Amount Spent vs. Salary')
    plt.show();
    
    plt.scatter(data.Catalogs,data.AmountSpent)
    plt.xlabel('Catalogs')
    plt.ylabel('Amount Spent');
    plt.title('Amount Spent vs. Catalogs')
    plt.show();
    
    plt.scatter(data.Catalogs,data.Salary)
    plt.xlabel('Catalogs')
    plt.ylabel('Salary');
    plt.title('Salary vs. Catalogs')
    plt.show();



def get_clean_df_labels(df):
    """Function takes original dataframe and returns cleaned data with labels.
    Dataframe will encode categorical variables, normalize data, and bin the amount spent into low, medium, and high
    resulting in a labels column."""

    from sklearn.preprocessing import OneHotEncoder
    from sklearn import preprocessing

    # ---ENCODE CATEGORIES---

    # creating instance of one-hot-encoder
    enc = OneHotEncoder(handle_unknown="ignore")

    # Age
    age = pd.DataFrame(enc.fit_transform(df[["Age"]]).toarray())
    age.columns = enc.get_feature_names(["Age"])

    # Gender
    gender = pd.DataFrame(enc.fit_transform(df[["Gender"]]).toarray())
    gender.columns = enc.get_feature_names(["Gender"])

    # OwnHome
    ownhome = pd.DataFrame(enc.fit_transform(df[["OwnHome"]]).toarray())
    ownhome.columns = enc.get_feature_names(["OwnHome"])

    # Married
    married = pd.DataFrame(enc.fit_transform(df[["Married"]]).toarray())
    married.columns = enc.get_feature_names(["Married"])

    # Location
    location = pd.DataFrame(enc.fit_transform(df[["Location"]]).toarray())
    location.columns = enc.get_feature_names(["Location"])

    # drop old columns
    df.drop(["Age", "Gender", "OwnHome", "Married", "Location", "History"], axis=1, inplace=True)

    # concat encoded columns
    df = pd.concat([df, age, gender, ownhome, married, location], axis=1)

    # ---CREATE BINS---
    AmountSpent = pd.Series(df['AmountSpent'])

    # median
    lower_bound = AmountSpent.median()

    # 3rd Quantile
    middle_bound = AmountSpent.quantile(0.75)

    # infinity
    upper_bound = 999999999

    bins = [0, lower_bound, middle_bound, upper_bound]

    # low, medium, high
    names = [0, 1, 2]

    # create labels
    Spend_labels = pd.cut(AmountSpent, bins, right=False, labels=names)

    # drop amount spent from df
    df.drop(['AmountSpent'], axis=1, inplace=True)

    # ---NORMALIZE DATA---

    # store the column names of df
    store_cols = df.columns

    # normalize data
    min_max_scaler = preprocessing.MinMaxScaler()
    data_scaled = min_max_scaler.fit_transform(df)
    DirectMarketingScaled = pd.DataFrame(data_scaled, columns=store_cols)

    # concat the df and labels
    DirectMarketing_Cln_Labels = pd.concat([DirectMarketingScaled, Spend_labels], axis=1)

    return DirectMarketing_Cln_Labels


