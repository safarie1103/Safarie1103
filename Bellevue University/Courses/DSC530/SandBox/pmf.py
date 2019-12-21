import pandas as pd
import matplotlib.pyplot as plt
# A die is rolled 40 times
Roll40times = [1,1,6,3,2,3,4,2,3,5,5,6,5,5,5,5,5,2,2,3,3,5,2,2,5,6,2,2,2,3,6,6,2,4,3,2,3,1,2,3]

Roll40times_df = pd.DataFrame(Roll40times)

Roll40times_series = Roll40times_df[0].value_counts()
Roll40times_series
total_num_of_events = len(Roll40times_series)
total_num_of_events
Roll40times_hist = pd.DataFrame(Roll40times_series)
Roll40times_hist
Roll40times_hist = Roll40times_hist.rename(columns={0:'event_count'})
Roll40times_hist
Roll40times_hist['event'] = Roll40times_hist.index
Roll40times_hist
# THE PMF function:
# The probability of each item is the number of
# Occurrence(frequency) of each item divided by total number of occurrences.
Roll40times_hist['event_probability'] = Roll40times_hist['event_count']/total_num_of_events
Roll40times_hist

fig,ax = plt.subplots(2,1)
# Histogram
ax[0].bar(Roll40times_hist['event'],Roll40times_hist['event_count'])
ax[0].set_xlabel("Roll Value (1-6)")
ax[0].set_ylabel("Number of occurrences")
ax[0].set_title("Histogram")
# PMF Graph
ax[1].bar(Roll40times_hist['event'],Roll40times_hist['event_probability'])
ax[1].set_xlabel("Roll Value (1-6)")
ax[1].set_ylabel("Probability of occurrences")
ax[1].set_title("PMF")
plt.show()

