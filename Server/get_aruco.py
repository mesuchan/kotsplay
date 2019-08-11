import cv2
import cv2.aruco as aruco
import json
import alchemy

# Center of polygon
def center_points(points):
    x = 0
    y = 0
    for i in points:
        x += i[0]
        y += i[1]

    return (int(x / len(points)), int(y / len(points)))

# Distance^2
def dist(p1, p2):
    return (p1[0] - p2[0]) ** 2 + (p1[1] - p2[1]) ** 2

# Find nearest table's slot for item
def get_slot(table, point):
    dists = [dist(t, point) for t in table]

    min_1 = min(dists)
    min_2 = min(dists[:dists.index(min_1)] + dists[dists.index(min_1)+1:])


    a = dists.index(min_1)
    b = dists.index(min_2)

    if a > b:
        a, b = b, a


    if a == 0 and b == 5:
        return 0
    elif a == 0 and b == 1:
        return 1
    elif a == 1 and b == 2:
        return 2
    elif a == 2 and b == 3:
        return 3
    elif a == 3 and b == 4:
        return 4
    elif a == 4 and b == 5:
        return 5
    else:
        return -1

# Scan image and return content of the image (table, items etc)
def return_scan(image):
    aruco_dict = aruco.Dictionary_get(aruco.DICT_4X4_50)
    parameters = aruco.DetectorParameters_create()
    
    corners, ids, rejectedImgPoints = aruco.detectMarkers(image, aruco_dict, parameters=parameters)
    corners = [corner.reshape(-1, 2).tolist() for corner in corners]

    ids = ids.reshape(1, -1).tolist()[0] if ids is not None else []

    table = [0 for i in range(6)]
    items = []

    for i, points in zip(ids, corners):
        if i in range(6):
            table[i] = center_points(points)
        else:
            items.append({"id": i, "center": center_points(points)})

    correct_table = 0 not in table and len(items) < 7

    slots = [0 for i in range(6)]

    if correct_table:
        for item in items:
            slot = get_slot(table, item["center"])
            if slot != -1:
                if slots[slot] == 0:
                    slots[slot] = item["id"]
                else:
                    slots[slot] = -1
                
    
    return {"correct": correct_table, "table": table, "items": items, "slots": slots}

# Examples of reading
##print(return_scan(cv2.imread("test_marker.jpg")))
##print(return_scan(cv2.imread("test_0.jpg")))
##print(return_scan(cv2.imread("test_1.jpg")))
##print(return_scan(cv2.imread("test_2.jpg")))
##print(return_scan(cv2.imread("test_3.jpg")))
##print(return_scan(cv2.imread("test_4.jpg")))


# Main image processing (for presentation)

# 0 - first webcam; if several in system - try another id (1, 2 etc.)
cap = cv2.VideoCapture(0)

# Already used items ids
used = set()

# Current frame in sequence of 15
num = 1

# Empty result
scan = {"correct": False, "table": [0,0,0,0,0,0], "items": [], "slots": [0,0,0,0,0,0]}

font = cv2.FONT_HERSHEY_SIMPLEX

# Load game receipts
book = alchemy.load_book("book.xml")

is_res = 0

# Common cycle of application
while(True):
    # Capture frame-by-frame
    ret, frame = cap.read()
    image = frame

    # Parse only 1 of 15 frames (for speed)
    if num % 15 == 0:
        if is_res:
            is_res -= 1
        num = 1
        scan = return_scan(frame)
    else:
        num += 1

    # Draw table
    if scan["correct"]:
        frame = cv2.line(frame, scan["table"][0], scan["table"][1], (255, 0, 0), 10)
        frame = cv2.line(frame, scan["table"][1], scan["table"][2], (255, 0, 0), 10)
        frame = cv2.line(frame, scan["table"][2], scan["table"][3], (255, 0, 0), 10)
        frame = cv2.line(frame, scan["table"][3], scan["table"][4], (255, 0, 0), 10)
        frame = cv2.line(frame, scan["table"][4], scan["table"][5], (255, 0, 0), 10)
        frame = cv2.line(frame, scan["table"][5], scan["table"][0], (255, 0, 0), 10)

    # Draw items markers and names
    if True:
        for i in scan["items"]:
            frame = cv2.circle(frame, i["center"], 5, (0, 0, 255), 10)

        for i in scan["items"]:
            iid = i["id"]
            if iid in used:
                # Already used
                frame = cv2.putText(frame, "X", i["center"], font, 0.75,
                                (0, 10, 200), 3)
            else:
                text = "???" # default = unknown name
                for ing in book.ingredients:
                    if ing.id == iid:
                        text = ing.name
                frame = cv2.putText(frame, text, i["center"], font, 0.75,
                                (0, 150, 0), 3)

    # Draw craft result
    if is_res > 0:
        if res is None:
            frame = cv2.putText(frame, "Recept failed!", (150, 300), font, 2,
                                (10, 0, 200), 3)
        else:
            frame = cv2.putText(frame, res.name, (200, 300), font, 2,
                                (170, 0, 0), 3)

    # Show final image
    cv2.imshow('frame', frame)
    
    # q = quit
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

    # r = reset used
    if cv2.waitKey(1) & 0xFF == ord('r'):
        print("resetted")
        used = set()

    # c - craft (if table fully visible + items in correct places)
    if cv2.waitKey(1) & 0xFF == ord('c') and is_res == 0 and scan["correct"]:
        if len(used.intersection(set([i["id"] for i in scan["items"]]))):
            res = None
        else:
            res = alchemy.find_recipe(book, set([i["id"] for i in scan["items"]]))
        used.update(set([i["id"] for i in scan["items"]]))
        is_res = 5
        
 
# When everything done, release the capture
cap.release()
cv2.destroyAllWindows()
