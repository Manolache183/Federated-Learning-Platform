from typing import List, Any, Tuple
from functools import reduce
import numpy as np
import numpy.typing as npt

NDArray = npt.NDArray[Any]
NDArrays = List[NDArray]

def aggregate(results: List[Tuple[NDArrays, int, float]]) -> NDArrays:
    # Compute total number of examples
    num_examples_total = sum([num_examples for _, num_examples, _ in results])
    accuracies_total = sum([num_examples * accuracy for _, num_examples, accuracy in results])



    weighted_weights = [
        [layer * num_examples for layer in weights] for weights, num_examples, _ in results
    ]

    # Compute weighted average
    weights_final: NDArrays = [
        reduce(np.add, layer_updates) / num_examples_total
        # unpack the list of layer updates into separate arguments
        for layer_updates in zip(*weighted_weights)
    ]

    accuracy_final = accuracies_total / num_examples_total

    return weights_final, accuracy_final
