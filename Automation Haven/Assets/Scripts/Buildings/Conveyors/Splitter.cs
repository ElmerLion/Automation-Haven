using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

[RequireComponent(typeof(ConveyerBelt))]
public class Splitter : MonoBehaviour {

    [SerializeField] private Transform[] outputPointList;


    private List<ItemObject> itemQueue;
    private ConveyerBelt conveyorBelt;
    private float distanceBetweenItems;
    private int nextOutputIndex = 0;
    private float lastItemOutputTime; 
    private float cooldownTime = 0.2f;

    private void Awake() {
        conveyorBelt = GetComponent<ConveyerBelt>();
        itemQueue = new List<ItemObject>();
    }

    private void Start() {
        conveyorBelt.OnItemAdded += OnItemAdded;
        conveyorBelt.OnItemRemoved += OnItemRemoved;
        distanceBetweenItems = conveyorBelt.GetDistanceBetweenItems();
        lastItemOutputTime = Time.time;

        InvokeRepeating(nameof(TryOutputItems), 0, 1f);
    }

    private void OnItemAdded(ItemObject item) {
        itemQueue.Add(item);

        TryOutputItems();
    }

    private void OnItemRemoved(ItemObject item) {
        itemQueue.Remove(item);
    }

    private void TryOutputItems() {
        if (Time.time - lastItemOutputTime < cooldownTime || itemQueue.Count == 0) {
            return; // Exit if it's too soon to process another item or the queue is empty
        }

        ItemObject item = itemQueue[0];

        bool outputAAvailable = IsOutputAvailable(outputPointList[0]) && !IsOutputSpaceOccupied(outputPointList[0].position + (outputPointList[0].forward * distanceBetweenItems), outputPointList[0]);
        bool outputBAvailable = outputPointList.Length > 1 && IsOutputAvailable(outputPointList[1]) && !IsOutputSpaceOccupied(outputPointList[1].position + (outputPointList[1].forward * distanceBetweenItems), outputPointList[1]);

        if (!outputAAvailable && !outputBAvailable) {
            return; 
        }

        // Determine which output point to use based on availability
        Transform outputPoint;
        if (outputAAvailable && (!outputBAvailable || nextOutputIndex == 0)) {
            outputPoint = outputPointList[0];
            nextOutputIndex = 1; // Alternate to the second output next time
        } else if (outputBAvailable) {
            outputPoint = outputPointList[1];
            nextOutputIndex = 0; // Alternate to the first output next time
        } else {
            return; // Exit if neither output is valid
        }

        if (TryOutputItem(item, outputPoint)) {
            itemQueue.RemoveAt(0); // Remove the item from the queue after successful output
            lastItemOutputTime = Time.time;
        }
    }

    private bool TryOutputItem(ItemObject item, Transform outputPoint) {
        Vector3 positionAfterOutputPoint = outputPoint.position + (outputPoint.forward * distanceBetweenItems);

        if (IsOutputAvailable(outputPoint) && !IsOutputSpaceOccupied(positionAfterOutputPoint, outputPoint)) {
            item.transform.position = outputPoint.position;
            item.transform.rotation = outputPoint.rotation;
            return true;
        }
        return false;
    }

    private bool IsOutputAvailable(Transform outputPoint) {
        Vector3 positionAfterOutputPoint = outputPoint.position + outputPoint.forward;

        PlacedObject_Done nextPlacedObjectDone = PlacedBuildingManager.Instance.GetPlacedObjectInCell(positionAfterOutputPoint);
        return PlacedBuildingManager.Instance.IsNextObjectConveyorBelt(nextPlacedObjectDone);
    }

    private bool IsOutputSpaceOccupied(Vector3 positionAfterOutputPoint, Transform outputPoint) {
        // Check if the space after the output point is occupied
        Collider[] hitColliders = Physics.OverlapSphere(positionAfterOutputPoint, distanceBetweenItems);
        foreach (Collider hitCollider in hitColliders) {
            if (hitCollider.transform == this || hitCollider.GetComponent<ItemObject>() != null) {
                return true; // Space is occupied
            }
        }
        return false; // Space is not occupied
    }

    public ConveyerBelt GetConveyorBelt() {
        return conveyorBelt;
    }

}
