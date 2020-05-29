def main():
    import gmaps
    import json
    with open('../APIkeys/APIkeys.json') as f:
        keys = json.load(f)
        key = keys['googlemaps']['key']

    gmaps.configure(api_key=key)
    fig = gmaps.figure()
    fig

    nyc = (40.75, -74.0)

    # %%

    gmaps.figure(center=nyc, zoom_level=12)
    gmaps

    # %%

    #gmaps.figure(map_type="HYBRID")



# Execute main function is this file is primary.
if __name__ == '__main__':
    main()
else:
    print("This Module's name is :" + __name__)